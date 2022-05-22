import { createContext } from "react";
import { ViewModels } from "../types/viewModels";


interface UserContextState {
    profileData?: ViewModels.ProfileViewModel;
    setProfileData: (profileData?: ViewModels.ProfileViewModel) => void;
}
const UserContext = createContext<UserContextState>({
    setProfileData: () => {}
});
export const UserProvider = UserContext.Provider;
export default UserContext;