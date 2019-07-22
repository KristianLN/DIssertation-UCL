import tensorflow as tf
import os
import numpy as np
import re
import csv

# Getting the files of interest.
summaryFiles = os.listdir()
# Disregarded the CSV generated files.
summaryFiles = [file for file in summaryFiles if "csv" not in file]

tag_names = {"Environment/Cumulative Reward":1,"Environment/Episode Length":2}

# Time to generate some CSV files.

for file in summaryFiles:

    eventfile = os.listdir(folder+file)
    filePath = folder+file+"/"+eventfile[0]
    # Store the data.
    data = [[] for tag in np.arange(len(list(tag_names.keys()))+1)]

    # Extracting the data.
    for event in tf.train.summary_iterator(filePath):

        # Storing the step.
        data[0].append(event.step)

        # Storing the values of interest.
        for value in event.summary.value:

            if value.tag in list(tag_names.keys()):

                data[tag_names[value.tag]].append(value.simple_value)

    data[0] = data[0][2:]
    # Preparing to write out the csv file.
    header = ["Step"]

    for tag in list(tag_names.keys()):
        # Removing the overall tag, which is the first in the tag_name and continuing with the actual
        # tag name.
        header.append(re.sub(" ","_",re.split("/",tag)[1]))

    with open("csv_output/"+file+".csv", "w") as outputfile:
        writer = csv.writer(outputfile)
        # Adding the header
        writer.writerow(header)
        # Adding the observations
        for obs in np.arange(len(data[0])):
            writer.writerow([newEle[obs] for newEle in data])
