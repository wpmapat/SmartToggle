import { useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";
import CompaniesPage from "./pages/CompaniesPage";
import ServicesPage from "./pages/ServicesPage";
import FeatureFlagsPage from "./pages/FeatureFlagsPage";
import "./App.css";

function App() {
    const isAuthenticated = useIsAuthenticated();
    const { instance } = useMsal();

    useEffect(() => {
        instance.handleRedirectPromise().catch(() => {});
    }, [instance]);

    const handleLogin = () => {
        instance.loginRedirect(loginRequest);
    };

    const handleLogout = () => {
        instance.logoutRedirect();
    };

    if (!isAuthenticated) {
        return (
            <div className="login-container">
                <div className="login-card">
                    <div className="login-logo">⚡</div>
                    <h1 className="login-title">SmartToggle</h1>
                    <p className="login-tagline">Feature Flag Management</p>
                    <p className="login-description">
                        Control your features in real time. Enable or disable functionality
                        across services without redeploying your application.
                    </p>
                    <button className="login-btn" onClick={handleLogin}>
                        <img src="https://learn.microsoft.com/en-us/entra/identity-platform/media/howto-add-branding-in-apps/ms-symbollockup_mssymbol_19.svg" alt="" width="20" height="20" />
                        Sign in with Microsoft
                    </button>
                </div>
            </div>
        );
    }

    return (
        <BrowserRouter>
            <nav className="navbar">
                <span className="nav-brand">SmartToggle</span>
                <button onClick={handleLogout}>Sign out</button>
            </nav>
            <div className="container">
                <Routes>
                    <Route path="/" element={<Navigate to="/companies" />} />
                    <Route path="/companies" element={<CompaniesPage />} />
                    <Route path="/companies/:companyId/services" element={<ServicesPage />} />
                    <Route path="/companies/:companyId/services/:serviceId/flags" element={<FeatureFlagsPage />} />
                </Routes>
            </div>
        </BrowserRouter>
    );
}

export default App;
