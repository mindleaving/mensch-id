export const AnyStatusOption = '';

export const buildFilterQueryParameters = (searchText: string, status: string): QueryParameters => {
    const parameters: { [key: string]: string } = {};
    const trimmedSearchText = searchText.trim();
    if(trimmedSearchText) {
        parameters.searchText = trimmedSearchText;
    }
    if(status !== AnyStatusOption) {
        parameters.status = status;
    }
    return parameters;
};
