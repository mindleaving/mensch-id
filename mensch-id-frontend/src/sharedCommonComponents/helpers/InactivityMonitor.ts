export class InactivityMonitor {
    timeoutHandle?: number;
    inactivityTimeoutInSeconds: number;
    inactivityCallbacks: Array<() => void>;

    constructor(
        inactivityTimeoutInSeconds: number
    ) {
        this.inactivityTimeoutInSeconds = inactivityTimeoutInSeconds;
        this.inactivityCallbacks = [];
    }

    registerCallback = (
        callback: () => void
    ) => {
        if(this.inactivityCallbacks.indexOf(callback) >= 0) {
            return;
        }
        this.inactivityCallbacks.push(callback);
    }

    unregisterCallback = (
        callback: () => void
    ) => {
        const index = this.inactivityCallbacks.indexOf(callback);
        if(index < 0) {
            return;
        }
        this.inactivityCallbacks.splice(index, 1);
    }

    onInactivity = () => {
        for (const callback of this.inactivityCallbacks) {
            callback();
        }
    }

    onActivity = () => {
        if(this.timeoutHandle) {
            clearTimeout(this.timeoutHandle);
        }
        this.timeoutHandle = setTimeout(this.onInactivity, this.inactivityTimeoutInSeconds * 1000);
    }

    start = () => {
        window.addEventListener('click', this.onActivity);
        window.addEventListener('keyup', this.onActivity);

        this.timeoutHandle = setTimeout(this.onInactivity, this.inactivityTimeoutInSeconds * 1000);
    }

    stop = () => {
        if(this.timeoutHandle) {
            clearTimeout(this.timeoutHandle);
        }

        window.removeEventListener('click', this.onActivity);
        window.removeEventListener('keyup', this.onActivity);
    }
}

export const inactivityMonitor: { instance?: InactivityMonitor } = {};