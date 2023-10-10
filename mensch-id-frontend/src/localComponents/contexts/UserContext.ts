import { createContext } from "react";
import { ViewModels } from "../types/viewModels";

const UserContext = createContext<ViewModels.IUserViewModel | undefined>(undefined);
export const UserProvider = UserContext.Provider;
export default UserContext;