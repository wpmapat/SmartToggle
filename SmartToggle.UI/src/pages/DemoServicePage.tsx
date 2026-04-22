import { useEffect, useState } from "react";

const SERVICE_ID = "2904c6b6-1c92-4f42-830d-bd11a2909638";
const COMPANY_ID = "e7ec5022-291b-4b7c-8d64-413a81696733";
const API_BASE = "https://smarttoggle-api.azurewebsites.net";

interface FeatureFlag {
    flagId: string;
    defaultValue: boolean;
}

export default function DemoServicePage() {
    const [flags, setFlags] = useState<Record<string, boolean>>({});
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [error, setError] = useState("");

    const loadFlags = async (isRefresh = false) => {
        if (isRefresh) setRefreshing(true);
        else setLoading(true);
        setError("");
        try {
            const response = await fetch(`${API_BASE}/api/public/featureflags/${SERVICE_ID}`);
            if (!response.ok) {
                const body = await response.text();
                throw new Error(`API returned ${response.status}: ${body}`);
            }
            const data: FeatureFlag[] = await response.json();
            const flagMap: Record<string, boolean> = {};
            data.forEach(f => { flagMap[f.flagId] = f.defaultValue; });
            setFlags(flagMap);
        } catch (err: any) {
            setError(err.message || "Failed to load feature flags.");
        } finally {
            setLoading(false);
            setRefreshing(false);
        }
    };

    useEffect(() => {
        loadFlags();
        const interval = setInterval(() => loadFlags(true), 10000);
        return () => clearInterval(interval);
    }, []);

    const isDarkTheme = flags["dark-theme"] ?? false;
    const isLargeFont = flags["large-font"] ?? false;

    const containerStyle: React.CSSProperties = {
        minHeight: "100vh",
        background: isDarkTheme ? "#1e293b" : "#f8fafc",
        color: isDarkTheme ? "#f1f5f9" : "#1e293b",
        fontSize: isLargeFont ? "1.25rem" : "1rem",
        padding: "40px",
        transition: "all 0.3s ease",
    };

    const cardStyle: React.CSSProperties = {
        maxWidth: "600px",
        margin: "0 auto",
        background: isDarkTheme ? "#334155" : "#ffffff",
        borderRadius: "16px",
        padding: "32px",
        boxShadow: "0 4px 24px rgba(0,0,0,0.1)",
    };

    const badgeStyle = (on: boolean): React.CSSProperties => ({
        display: "inline-block",
        padding: "4px 12px",
        borderRadius: "12px",
        fontSize: "0.8rem",
        fontWeight: 700,
        background: on ? "#22c55e" : "#e2e8f0",
        color: on ? "white" : "#64748b",
        marginLeft: "8px",
    });

    if (loading) return <div style={containerStyle}><p>Loading flags...</p></div>;
    if (error) return <div style={containerStyle}><p style={{ color: "#ef4444" }}>{error}</p></div>;

    return (
        <div style={containerStyle}>
            <div style={cardStyle}>
                <h1 style={{ margin: "0 0 8px", fontSize: isLargeFont ? "2rem" : "1.5rem" }}>
                    Demo Service
                </h1>
                <p style={{ color: isDarkTheme ? "#94a3b8" : "#64748b", margin: "0 0 24px" }}>
                    This page represents a real-world service that reads its configuration from SmartToggle at runtime — no redeployment needed to change its behavior.
                </p>

                <div style={{
                    background: isDarkTheme ? "#1e293b" : "#f1f5f9",
                    border: `1px solid ${isDarkTheme ? "#475569" : "#e2e8f0"}`,
                    borderRadius: "12px",
                    padding: "20px",
                    marginBottom: "32px",
                }}>
                    <p style={{ margin: "0 0 12px", fontWeight: 600, fontSize: "0.9rem", color: isDarkTheme ? "#94a3b8" : "#475569", textTransform: "uppercase", letterSpacing: "0.5px" }}>How it works</p>
                    <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                        {[
                            "Click \"Manage flags in SmartToggle\" below",
                            "Toggle dark-theme or large-font on or off",
                            "Come back — the page auto-refreshes every 10 seconds",
                        ].map((step, i) => (
                            <div key={i} style={{ display: "flex", alignItems: "center", gap: "12px" }}>
                                <span style={{
                                    minWidth: "24px", height: "24px", borderRadius: "50%",
                                    background: "#2563eb", color: "white",
                                    display: "flex", alignItems: "center", justifyContent: "center",
                                    fontSize: "0.75rem", fontWeight: 700,
                                }}>{i + 1}</span>
                                <span style={{ fontSize: "0.9rem", color: isDarkTheme ? "#cbd5e1" : "#475569" }}>{step}</span>
                            </div>
                        ))}
                    </div>
                </div>

                <h3 style={{ margin: "0 0 16px" }}>Active Feature Flags</h3>
                <div style={{ display: "flex", flexDirection: "column", gap: "12px", marginBottom: "32px" }}>
                    {Object.entries(flags).map(([key, value]) => (
                        <div key={key} style={{ display: "flex", alignItems: "center" }}>
                            <span>{key}</span>
                            <span style={badgeStyle(value)}>{value ? "ON" : "OFF"}</span>
                        </div>
                    ))}
                </div>

                <div style={{
                    padding: "20px",
                    borderRadius: "12px",
                    background: isDarkTheme ? "#1e293b" : "#f1f5f9",
                    border: `2px solid ${isDarkTheme ? "#475569" : "#e2e8f0"}`,
                    marginBottom: "24px",
                }}>
                    <p style={{ margin: 0 }}>
                        {isDarkTheme && isLargeFont && "🌙 Dark theme + Large font are both ON"}
                        {isDarkTheme && !isLargeFont && "🌙 Dark theme is ON — toggle large-font to increase text size"}
                        {!isDarkTheme && isLargeFont && "🔠 Large font is ON — toggle dark-theme for a dark background"}
                        {!isDarkTheme && !isLargeFont && "All flags are OFF — go to SmartToggle and toggle some flags to see changes here!"}
                    </p>
                </div>

                <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
                    <a
                        href={`/companies/${COMPANY_ID}/services/${SERVICE_ID}/flags`}
                        style={{
                            display: "inline-block",
                            padding: "10px 20px",
                            background: "#2563eb",
                            color: "white",
                            borderRadius: "8px",
                            textDecoration: "none",
                            fontSize: "0.95rem",
                            fontWeight: 500,
                        }}
                    >
                        Manage flags in SmartToggle →
                    </a>
                    <button
                        onClick={() => loadFlags(true)}
                        disabled={refreshing}
                        style={{
                            padding: "10px 20px",
                            background: isDarkTheme ? "#0f766e" : "#0d9488",
                            color: "white",
                            border: "none",
                            borderRadius: "8px",
                            cursor: refreshing ? "not-allowed" : "pointer",
                            fontSize: "0.95rem",
                            fontWeight: 500,
                            opacity: refreshing ? 0.6 : 1,
                        }}
                    >
                        {refreshing ? "Refreshing..." : "↻ Refresh Flags"}
                    </button>
                </div>
            </div>
        </div>
    );
}
