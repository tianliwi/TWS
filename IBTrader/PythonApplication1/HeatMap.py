import matplotlib.pyplot as plt
import numpy as np
import csv

reader = csv.reader(open('E:\GitHub\TWS\Data\matrix_20170926_175944.csv') , delimiter=',')
x = list(reader)
res = np.array(x).astype("float")

plt.imshow(res, cmap='hot')
plt.colorbar()
plt.show()