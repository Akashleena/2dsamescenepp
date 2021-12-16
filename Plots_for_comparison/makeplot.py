import pandas as pd
 
import matplotlib.pyplot as plt

# import seaborn as sns


pp=pd.read_csv('/home/aleena/Unity/Hub/projects/A-Star-Pathfinding-Tutorial/Assets/testpp.csv')

ax = pp.plot.bar(x='NameofPP', y='TotalTime', rot=45)
plt.savefig('TotalTime.png')
ax = pp.plot.bar(x='NameofPP', y='TotalCost', rot=45)
plt.savefig('TotalCost.png')
ax = pp.plot.bar(x='NameofPP', y='TotalNodes', rot=45)
plt.savefig('TotalNodes.png')
ax = pp.plot.bar(x='NameofPP', y='TotalSimTime', rot=45)
plt.savefig('TotalTime.png')
plt.show()



