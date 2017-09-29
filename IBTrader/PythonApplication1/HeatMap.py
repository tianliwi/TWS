import matplotlib.pyplot as plt
import numpy as np
import csv

reader = csv.reader(open(r"C:\Users\liti\Documents\TWS\Data\matrix_20170928_163153.csv") , delimiter=',')
x = list(reader)
res = np.array(x).astype("float")

plt.imshow(res, cmap='hot')
plt.colorbar()
plt.show()