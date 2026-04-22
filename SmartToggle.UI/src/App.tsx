import { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate, useNavigate } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest, graphRequest, apiBaseUrl } from "./authConfig";
import CompaniesPage from "./pages/CompaniesPage";
import ServicesPage from "./pages/ServicesPage";
import FeatureFlagsPage from "./pages/FeatureFlagsPage";
import DemoServicePage from "./pages/DemoServicePage";
import "./App.css";

function AuthenticatedApp() {
    const { instance } = useMsal();
    const navigate = useNavigate();
    const [companyName, setCompanyName] = useState("");

    useEffect(() => {
        const provision = async () => {
            try {
                const graphToken = await instance.acquireTokenSilent({
                    ...graphRequest,
                    account: instance.getAllAccounts()[0],
                });
                const graphRes = await fetch("https://graph.microsoft.com/v1.0/organization", {
                    headers: { Authorization: `Bearer ${graphToken.accessToken}` },
                });
                const graphData = await graphRes.json();
                const orgName = graphData.value?.[0]?.displayName ?? "My Organization";

                const apiToken = await instance.acquireTokenSilent({
                    ...loginRequest,
                    account: instance.getAllAccounts()[0],
                });
                const provisionRes = await fetch(`${apiBaseUrl}/api/company/provision`, {
                    method: "POST",
                    headers: {
                        Authorization: `Bearer ${apiToken.accessToken}`,
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ companyName: orgName }),
                });
                const company = await provisionRes.json();
                setCompanyName(company.name);
                navigate(`/companies/${company.id}/services`);
            } catch (err) {
                console.error("Provision failed:", err);
            }
        };
        provision();
    }, []);

    const handleLogout = () => {
        instance.logoutRedirect();
    };

    return (
        <>
            <nav className="navbar">
                <span className="nav-brand">SmartToggle</span>
                <div style={{ display: "flex", gap: "12px", alignItems: "center" }}>
                    {companyName && <span style={{ color: "#94a3b8", fontSize: "0.9rem" }}>{companyName}</span>}
                    <button onClick={() => navigate("/demo")} style={{ background: "#7c3aed", border: "none", color: "white", padding: "6px 14px", borderRadius: "6px", cursor: "pointer" }}>Live Demo</button>
                    <button onClick={handleLogout}>Sign out</button>
                </div>
            </nav>
            <div className="container">
                <Routes>
                    <Route path="/" element={<Navigate to="/companies" />} />
                    <Route path="/companies" element={<CompaniesPage />} />
                    <Route path="/companies/:companyId/services" element={<ServicesPage />} />
                    <Route path="/companies/:companyId/services/:serviceId/flags" element={<FeatureFlagsPage />} />
                    <Route path="/demo" element={<DemoServicePage />} />
                </Routes>
            </div>
        </>
    );
}

function App() {
    const isAuthenticated = useIsAuthenticated();
    const { instance } = useMsal();

    useEffect(() => {
        instance.handleRedirectPromise().catch(() => {});
    }, [instance]);

    const handleLogin = () => {
        instance.loginRedirect(loginRequest);
    };

    if (!isAuthenticated) {
        if (window.location.pathname === "/demo") {
            return <DemoServicePage />;
        }
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
            <AuthenticatedApp />
        </BrowserRouter>
    );
}

export default App;
