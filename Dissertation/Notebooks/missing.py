fig,ax1 = plt.subplots(figsize=(10,10))
colors = ["r","b","c","y","m","k","g"]
labels = ["S: 2, DI: 5, TS: 60",
         "S: 1, DI: 5, TS: 60",
         "S: 1, DI: 3, TS: 60",
         "S: 1, DI: 7, TS: 60",
         "S: 1, DI: 10, TS: 60",
         "S: 1, DI: 5, TS: 100",
         "S: 1, DI: 5, TS: 20"]

for i,folder in enumerate(folders):

    content_csv = pd.read_csv("../../summaries_for_processing/GoalBrain_4_2/csv_output/"+folder, index_col=0)
    if i == 0:
        step = np.array(content_csv.columns).astype(float)
    print("Folder processed: " + folder)
    print("Assosicated color: " + colors[i])
    print("-------------------------------------")
    print("Average Episode Length; Mean: %.3f, Std. Dev.: %.3f" % (content_csv.loc[2].mean(),\
                                                                   content_csv.loc[2].std()))
    print("Average cumulative reward; Mean (Of converged path): %.3f" % (content_csv.iloc[0,59:].mean()))
    print("Average cumulative reward; Std.Dev.: %.3f" % (content_csv.loc[1].std()))
    print("-------------------------------------\n")
    plt1 = ax1.plot(step,(content_csv.loc[1]/content_csv.loc[1].std()),color=colors[i],label=labels[i])
    ax1.set_xlabel("Number of Steps",fontsize = 12)
    ax1.set_ylabel("Average Cumulative Reward",fontsize=12)
    ax1.set_ylim((-2,2))
#     plt2 = ax2.plot(step,(content_csv.loc[1]),color=colors[i],label=labels[i])
#     ax2.set_xlabel("Number of Steps",fontsize = 12)
#     ax2.set_ylabel("Average Cumulative Reward",fontsize=12)
#     ax2.set_ylim((-1.2,1))
# ax2 = ax1.twinx()
# plt2 = ax2.plot(step,content_csv.loc[2],color='r',label="Episode")
# ax2.set_ylabel("Average Episode Length",fontsize=12)
# # Legends
#     if i == 0:
#         plts = plt1
#     else:
#         plts += plt1
# labels = [l.get_label() for l in plts]
plt.legend(labels,loc=0,bbox_to_anchor=(1,1),fontsize=12)#bbox_to_anchor=(1.35,1)

# plt.savefig("Graphs/BasicLearningEnvironment.jpg",bbox_inches="tight")
plt.show()
