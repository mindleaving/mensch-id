import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes"
import AboutPage from "../pages/AboutPage";
import ContactPage from "../pages/ContactPage";
import EventsPage from "../pages/EventsPage";
import PrivacyPage from "../pages/PrivacyPage";
import SendChallengePage from "../pages/SendChallengePage";
import TermsOfServicePage from "../pages/TermsOfServicePage";

export const CommonRoutes = () => {
    const routes: RouteDefinition[] = [
        { path: '/challenge', element: <SendChallengePage /> },
        { path: '/privacy', element: <PrivacyPage /> },
        { path: '/terms-of-service', element: <TermsOfServicePage /> },
        { path: '/about', element: <AboutPage /> },
        { path: '/events', element: <EventsPage /> },
        { path: '/contact', element: <ContactPage /> }
    ];
    return routes
}