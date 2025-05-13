import json
from turtle import color
from matplotlib import pyplot as plt
from matplotlib import colors as clrs
import numpy as np


def boxplots(obj):
    boxplot_data = []
    labels = []

    for result in sorted(obj["results"], key=lambda x: x["name"]):
        # Boxplot for mean result of each experiment
        boxplot_data.append(result["results"])
        labels.append(result["name"])

    plt.boxplot(boxplot_data, tick_labels=labels)
    plt.title("Experiment Results")
    plt.xticks(rotation=11)
    plt.ylabel("Amount saved ($)")


def performance_line_chart(obj):
    fig, axs = plt.subplots()

    for result in obj["results"]:
        max_y_vals = result["maxOverTime"]
        min_y_vals = result["minOverTime"]
        max_y_vals_clamped = max_y_vals[: min(len(max_y_vals), len(min_y_vals))]
        min_y_vals_clamped = min_y_vals[: min(len(max_y_vals), len(min_y_vals))]
        x_vals = np.arange(0, len(max_y_vals))

        color = axs.plot(x_vals, max_y_vals)[0].get_color()
        rgb = clrs.to_rgb(color)
        lighten_amount = 0.3
        rgb_lighter = (
            min(rgb[0] + lighten_amount, 1),
            min(rgb[1] + lighten_amount, 1),
            min(rgb[2] + lighten_amount, 1),
        )
        alpha = 0.8

        axs.plot(x_vals, min_y_vals, color=color, label=result["name"])
        axs.fill_between(
            x_vals,
            max_y_vals_clamped,
            min_y_vals_clamped,
            interpolate=True,
            color=rgb_lighter,
            alpha=alpha,
        )

        plt.title("Performance over time")
        plt.xlabel("Running time (s)")
        plt.ylabel("Amount saved ($)")
        plt.legend(loc="upper left")


path = input("Please enter the results file: ")
option = int(
    input(
        "Please enter the visualization you wish to see (1 = boxplots, 2 = performance line chart): "
    )
)
f = open(path, "r")
obj = json.loads(f.read())

if option == 1:
    boxplots(obj)
elif option == 2:
    performance_line_chart(obj)

plt.show()
f.close()
