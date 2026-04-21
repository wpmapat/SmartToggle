import { useEffect, useState } from "react";

const SERVICE_ID = "2904c6b6-1c92-4f42-830d-bd11a2909638";
const API_BASE = "https://smarttoggle-api.azurewebsites.net";

interface FeatureFlag {
    flagId: string;
    defaultValue: boolean;
}

export default function DemoServicePage() {
    const [flags, setFlags] = useState<Record<string, boolean>>({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadFlags = async () => {
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
            }
        };

        loadFlags();
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
                <p style={{ color: isDarkTheme ? "#94a3b8" : "#64748b", margin: "0 0 32px" }}>
                    This page reads its feature flags from SmartToggle and changes its appearance in real time.
                </p>

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
                }}>
                    <p style={{ margin: 0 }}>
                        {isDarkTheme && isLargeFont && "🌙 Dark theme + Large font are both ON"}
                        {isDarkTheme && !isLargeFont && "🌙 Dark theme is ON — toggle large-font to increase text size"}
                        {!isDarkTheme && isLargeFont && "🔠 Large font is ON — toggle dark-theme for a dark background"}
                        {!isDarkTheme && !isLargeFont && "All flags are OFF — go to SmartToggle and toggle some flags to see changes here!"}
                    </p>
                </div>
            </div>
        </div>
    );
}
