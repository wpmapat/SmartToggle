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
                <h1>SmartToggle</h1>
                <p>Feature Flag Management</p>
                <button onClick={handleLogin}>Sign in with Microsoft</button>
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
