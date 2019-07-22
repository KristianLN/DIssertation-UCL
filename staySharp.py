import os
import numpy as np
import time

# Future development:
#
# * Add degree of difficulty
# * Let the user specify a frequency (low,medium,high) and draw a number within some bounded range.

def mathEngine():

    files = os.listdir()
    
    if "results.txt" not in files:
        with open("results.txt","w") as file:
            file.write("Number,Correct,First_number,Second_number,Time")
    # runTime = int(input("For how long (in minutes) should I provide you with challenges?"))*60
    #
    # howMany = int(input("How many challenges are you looking to get?"))
    #
    # occurences = sorted(np.random.randint(0,runTime,howMany))
    #
    # keepTrack = [False for occurence in occurences]
    #
    # degreeOfFun = 0

    with open("results.txt","r") as file:
        number = len(file.readlines()) - 1

    # start = time.time()
    #
    # print(occurences)

    # while(any(keepTrack)==False):
    #
    #     if degreeOfFun == 0:
    #
    #         shift = 0
    #     else:
    #
    #         shift = occurences[degreeOfFun - 1]

        # timeTracker = (time.time() - start) + shift

        # if ((timeTracker > occurences[degreeOfFun] * 0.98) and (timeTracker > occurences[degreeOfFun] * 1.02)):

    first_number = np.random.randint(10,99)
    second_number = np.random.randint(10,99)

    goo = time.time()

    answer = int(input("What is %i times %i? Answer: " % (first_number,second_number)))

    end = time.time()

    popUp = input("Your time was %.3f (enter any key to continue.)" % (end-goo))

    # Checking the result
    if answer == first_number * second_number:
        correct = 1
        print("\nCorrect!")
    else:
        correct = 0
        print("\nWrong! The answer is: %i" % (first_number * second_number))

    # Saving the result
    with open("results.txt","a") as file:
        file.write("\n"+str(number)+","+str(correct)+","+str(first_number)+","+str(second_number)\
                  +","+str(round((end-goo),2)))

            # keepTrack[degreeOfFun] = True
            # degreeOfFun += 1
            # number += 1
            # start = time.time()
