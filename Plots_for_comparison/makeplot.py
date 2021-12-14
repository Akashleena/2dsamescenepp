import pandas as pd
 
import matplotlib.pyplot as plt

import seaborn as sns

pp=pd.read_csv('/home/aleena/Unity/Hub/projects/A-Star-2d/Assets/testpp.csv')

#fig, axes = plt.subplots(nrows=2, ncols=2)
df = pd.DataFrame(pp)  
X = list(df.iloc[:, 0])
Y = list(df.iloc[:, 1])
plt.figure(figsize=(9,6)) 
plt.bar(X, Y, color='midnightblue')
plt.title('Total Time Taken by Planner', fontsize=16, fontweight='bold')
plt.xlabel('Path Planner')
plt.ylabel('Total time in seconds')

plt.xticks(rotation=45, fontsize=13)
sns.set()
plt.savefig('totaltimeplot.png')

X = list(df.iloc[:, 0])
Y = list(df.iloc[:, 2])
plt.figure(figsize=(9,6)) 
plt.bar(X, Y, color='r')
plt.title('Average Time Taken by Planner', fontsize=16, fontweight='bold')
plt.xlabel('Path Planner')
plt.ylabel('Average time in seconds')

plt.xticks(rotation=45, fontsize=13)
sns.set()
plt.savefig('averagetimeplot.png')
plt.show()
