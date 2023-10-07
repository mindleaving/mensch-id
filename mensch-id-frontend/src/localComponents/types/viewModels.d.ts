import { Models } from './models.d';
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
