import matplotlib.pyplot as plt


def graph(loss):
    plt.plot(loss)
    plt.title('Model Loss / Epoch')
    plt.ylabel('Loss')
    plt.xlabel('Epoch')
    plt.legend(['train', 'test'], loc='lower left')
    plt.show()