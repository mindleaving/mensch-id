export enum SessionStoreKeys {
    UserViewModel = "UserViewModel"
}
export interface TimeRange {
    start?: Date;
    end?: Date;
}
export interface MenschIdCommunityEvent {
    name: string;
    startTime: Date;
    url?: string;
}