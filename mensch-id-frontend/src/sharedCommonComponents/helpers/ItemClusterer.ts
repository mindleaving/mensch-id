import { compareAsc, differenceInSeconds } from "date-fns";
import { TimeCluster } from "../types/frontendTypes";
import { last } from "./CollectionHelpers";

export const clusterByTime = <T>(items: T[], timeSelector: (item: T) => Date, clusterThresholdInSeconds: number): TimeCluster<T>[] => {
    if(!items || items.length === 0) {
        return [];
    }
    const itemsWithTime: { item: T, time: Date }[] = items.map(item => ({
        item: item,
        time: timeSelector(item)
    }));
    const timeSortedItems = itemsWithTime.sort((t1,t2) => compareAsc(t1.time, t2.time));
    if(timeSortedItems.length < 2) {
        return [ 
            {
                startTime: timeSortedItems[0].time,
                endTime: last(timeSortedItems)!.time,
                items: timeSortedItems.map(x => x.item)
            }
        ];
    }
    const clusters: TimeCluster<T>[] = [];
    let currentCluster: TimeCluster<T> = {
        startTime: timeSortedItems[0].time,
        endTime: timeSortedItems[0].time,
        items: [ timeSortedItems[0].item ]
    };
    clusters.push(currentCluster);
    for (const item of timeSortedItems.slice(1)) {
        const deltaT = differenceInSeconds(item.time, currentCluster.endTime);
        if(deltaT > clusterThresholdInSeconds) {
            currentCluster = {
                startTime: item.time,
                endTime: item.time,
                items: [ item.item ]
            };
            clusters.push(currentCluster);
        } else {
            currentCluster.endTime = item.time;
            currentCluster.items.push(item.item);
        }
    }
    return clusters;
}