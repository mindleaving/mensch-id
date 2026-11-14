import { Models, System } from './models.d';
import * as Enums from './enums';

export namespace ViewModels {
    interface AssignerAccountViewModel extends ViewModels.IUserViewModel {
        accountId: string;
        name: string;
        logoId?: string;
        logoImageType?: string;
        contactInformation: Models.Contact;
        preferedLanguage: Enums.Language;
    }
    interface IUserViewModel {
        userType: Enums.UserType;
    }
    interface LoginProviderViewModel {
        id: string;
        displayName: string;
    }
    interface NewProfileViewModel {
        idCandidates: string[];
    }
    interface ProfileViewModel extends ViewModels.IUserViewModel {
        id: string;
        loginProviders: string[];
        verifications: Models.Verification[];
    }
    interface ServiceChallengeViewModel {
        id: string;
        serviceDisplayName: string;
        serviceContactInformation: string;
        description: string;
        state: Enums.ServiceChallengeState;
        createdTimestamp: string;
        expiresAt: string;
    }
    interface SessionViewModel {
        id: string;
        type: Enums.SessionType;
        clientName: string;
        userAgent: string;
        createdTimestamp?: string | null;
        lastActivityTimestamp?: string | null;
        isCurrent: boolean;
    }
    interface SignedAssignerControlledProfile extends Models.AssignerControlledProfile {
        timestamp: string;
        signature: string;
    }
}
