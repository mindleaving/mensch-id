export enum SessionStoreKeys {
    AccessToken = "AccessToken"
}
export interface TimeRange {
    start?: Date;
    end?: Date;
}
export interface AssignedProfilesFilter {
    searchText?: string;
    timeRange?: TimeRange;
}