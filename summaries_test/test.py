import sys
import re
import numpy as np
print("This is a test, input: %s" % str(sys.argv[1]))

clean = re.split(",",re.sub("[:]",",",re.sub("[{}]","",sys.argv[1])))
print(clean)
tags = {}

for i in np.arange(len(clean),step = 2):
    tags[clean[i]] = clean[i + 1]
print(tags)
print(type(tags))
