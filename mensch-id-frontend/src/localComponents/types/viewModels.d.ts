import { Models } from './models.d';
import * as Enums from './enums.d';

export namespace ViewModels {
    interface AssignerAccountViewModel extends ViewModels.IUserViewModel {
        accountId: string;
        name: string;
        logoUrl: string;
        contactInformation: Models.Contact;
    }

    interface IUserViewModel {
        userType: Enums.UserType;
    }

    interface NewProfileViewModel {
        idCandidates: string[];
    }

    interface ProfileViewModel extends ViewModels.IUserViewModel {
        id: string;
        loginProviders: Enums.LoginProvider[];
        verifications: Models.Verification[];
    }

    interface SignedAssignerControlledProfile extends Models.AssignerControlledProfile {
        timestamp: Date;
        signature: string;
    }
}
