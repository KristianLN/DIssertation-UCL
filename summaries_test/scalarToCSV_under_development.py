import tensorflow as tf
import os
import numpy as np
import re
import csv
import pandas as pd
import sys
## Functions

def structureFiles(files):
    # Separating the run-id and run-num from the brain tag.
    fileNames = [re.split("_",file) for file in files]

    # Continuing with run-id and run-num
    cleanedFileNames = [file[0] for file in fileNames]

    # Storing brain tag for later use
    ending = fileNames[0][1:]
    ending = "_" + ending[0] + "_" + ending[1]

    # Creating a splitted list for each of the cleaned file name, for later comparison
    splittedCleanedName = [re.split("[-.]",file) for file in cleanedFileNames]

    # Structing the different datasets (event files from tensorflow).
    similar = []
    for i,name in enumerate(cleanedFileNames):

        if i == 0:

            similar.append([name])

        else:

            if len(splittedCleanedName[i]) == len(splittedCleanedName[i-1]):

                whereSimilar = [splittedCleanedName[i][j]==splittedCleanedName[i-1][j] for j in \
                                                                 np.arange(len(splittedCleanedName[i]))]

                pos = [l for l,ele in enumerate(whereSimilar) if ele == 0][0]

                if pos == (len(splittedCleanedName[i]) - 1):

                    similar[len(similar) - 1].append(name)

                else:

                    similar.append([name])

            else:

                similar.append([name])

    # returning the structured data.
    return similar,ending

def eventToCSV(file,tags,end, folder = None):#folder,
    # print(tags)
    # Getting the event file
    if folder == None:
        eventfile = os.listdir(file+end)#folder+
        filePath = file+end+"/"+eventfile[0]#folder+
    else:
        eventfile = os.listdir(folder+file+end)
        filePath = folder+file+end+"/"+eventfile[0]

    # Extracting the data.
    for event in tf.train.summary_iterator(filePath):
        # if "Hyperparameters" in event.summary.value:
        #     continue
        if ((event.step != 0) and (tags == None)):

            # print(event.summary.value)
                # getTags = tf.train.summary_iterator(filePath)[1]
            tags = np.unique([value.tag for value in event.summary.value])
            tags = {tags:i+1 for i,tags in enumerate(tags)}

            # Store the data.
            data = [[] for tag in np.arange(len(list(tags.keys()))+1)]
        elif (event.step != 0):
            # Store the data.
            data = [[] for tag in np.arange(len(list(tags.keys()))+1)]
            # Storing the step.
        if tags != None:
            data[0].append(event.step)

            # Storing the values of interest.
            for value in event.summary.value:

                if value.tag in list(tags.keys()):

                    data[tags[value.tag]].append(value.simple_value)
        # if ((tags == None) or (tags.size != 0)):
        #     # getTags = tf.train.summary_iterator(filePath)[1]
        #     tags = np.unique([value.tag for value in event.summary.value])
        #     print(tags)
        #     print(tags.size)
        # else:
        #     # Store the data.
        #     data = [[] for tag in np.arange(len(list(tags.keys()))+1)]
        # # Storing the step.
        # if tags != []:
        #     data[0].append(event.step)
        #
        #     # Storing the values of interest.
        #     for value in event.summary.value:
        #
        #         if value.tag in list(tags.keys()):
        #
        #             data[tag_names[value.tag]].append(value.simple_value)

    data[0] = data[0][2:]

    return data
## End of functions
##############################################################################################
## Let's process

if ((len(sys.argv) == 1) or (sys.argv[1] == None)):
    folder = None
    summaryFiles = os.listdir()
    output_dir = "csv_output"

    if output_dir not in os.listdir():
        os.mkdir(output_dir)
else:
    folder = sys.argv[1]
    summaryFiles = os.listdir(folder)
    output_dir = "csv_output"

    if output_dir not in os.listdir(folder):
        os.mkdir(folder+output_dir)
## Make it such that if tag_names isn't defined, the unique tags in the event file is used.
if len(sys.argv) >= 3:
    clean = re.split(",",re.sub("[:]",",",re.sub("[{}]","",sys.argv[1])))
    # print(clean)
    tag_names = {}

    for i in np.arange(len(clean),step = 2):
        if int(clean[i + 1]) == 0:
            raise ValueError("A tag is not allowed to have a start position of 0.")
        else:
            tag_names[clean[i]] = int(clean[i + 1])
    # tag_names
    # tag_names = {"Environment/Cumulative Reward":1,"Environment/Episode Length":2,
    #             "Environment/Lesson":3, "Losses/Forward Loss":4, "Losses/Inverse Loss":5,
    #             "Losses/Policy Loss":6, "Losses/Value Loss":7, "Policy/Curiosity Reward": 8,
    #             "Policy/Entropy":9, "Policy/Learning Rate":10, "Policy/Value Estimate": 11}
else:
    tag_names = None

summaryFiles = [file for file in summaryFiles if "py" not in file]
summaryFiles = [file for file in summaryFiles if "csv" not in file]

structuredFiles, ending = structureFiles(summaryFiles)

# # Preparing to write out the csv file.
# header = ["Step"]

# for tag in list(tag_names.keys()):
#     # Removing the overall tag, which is the first in the tag_name and continuing with the actual tag name.
#     header.append(re.sub(" ","_",re.split("/",tag)[1]))

for fileGroup in structuredFiles:
    print(fileGroup)
    if len(fileGroup) == 1:
        frame = pd.DataFrame(eventToCSV(fileGroup[0],tag_names,ending,folder))
        frame = frame.rename(columns=frame.iloc[0]).drop(frame.index[0])
        frame.to_csv(output_dir+"/"+fileGroup[0]+".csv")

    else:

        for i,f in enumerate(fileGroup):
            # tracker = np.arange(len(fileGroup)+len(tag_names)+1)

            if i == 0:

                frame = pd.DataFrame(eventToCSV(f,tag_names,ending,folder))
            else:

                frame_1 = pd.DataFrame(eventToCSV(f,tag_names,ending,folder))
                frame = pd.concat([frame,frame_1.loc[1:2]],ignore_index=True)

        frame = frame.rename(columns=frame.iloc[0]).drop(frame.index[0])
        frame.to_csv(output_dir+"/"+fileGroup[0]+".csv")
print("Processing is done..!")
