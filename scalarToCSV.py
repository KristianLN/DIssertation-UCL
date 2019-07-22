import tensorflow as tf
import os
import numpy as np
import re
import csv
import pandas as pd

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

def eventToCSV(file,tags,end):#folder,

    # Getting the event file
    eventfile = os.listdir(file+end)#folder+
    filePath = file+end+"/"+eventfile[0]#folder+

    # Store the data.
    data = [[] for tag in np.arange(len(list(tags.keys()))+1)]

    # Extracting the data.
    for event in tf.train.summary_iterator(filePath):

        # Storing the step.
        data[0].append(event.step)

        # Storing the values of interest.
        for value in event.summary.value:

            if value.tag in list(tags.keys()):

                data[tag_names[value.tag]].append(value.simple_value)

    data[0] = data[0][2:]

    return data
## End of functions
##############################################################################################
## Let's process

#folder = "summaries_test/"
summaryFiles = os.listdir()#folder
output_dir = "csv_output"

if output_dir not in os.listdir():#folder
    os.mkdir(output_dir)#folder+

tag_names = {"Environment/Cumulative Reward":1,"Environment/Episode Length":2}

summaryFiles = [file for file in summaryFiles if "py" not in file]
summaryFiles = [file for file in summaryFiles if "csv" not in file]

structuredFiles, ending = structureFiles(summaryFiles)

# Preparing to write out the csv file.
header = ["Step"]

for tag in list(tag_names.keys()):
    # Removing the overall tag, which is the first in the tag_name and continuing with the actual
    # tag name.
    header.append(re.sub(" ","_",re.split("/",tag)[1]))

for fileGroup in structuredFiles:

    if len(fileGroup) == 1:
        frame = pd.DataFrame(eventToCSV(fileGroup[0],tag_names,ending))#folder,|,columns = header
        frame = frame.rename(columns=frame.iloc[0]).drop(frame.index[0])
        frame.to_csv(output_dir+"/"+fileGroup[0]+".csv")

    else:

        for i,f in enumerate(fileGroup):
            tracker = np.arange(len(fileGroup)+len(tag_names)+1)

            if i == 0:

                frame = pd.DataFrame(eventToCSV(f,tag_names,ending))#folder,
            else:

                frame_1 = pd.DataFrame(eventToCSV(f,tag_names,ending))#folder,
                frame = pd.concat([frame,frame_1.loc[1:2]],ignore_index=True)

        frame = frame.rename(columns=frame.iloc[0]).drop(frame.index[0])
        frame.to_csv(output_dir+"/"+fileGroup[0]+".csv")
print("Processing is done..!")
