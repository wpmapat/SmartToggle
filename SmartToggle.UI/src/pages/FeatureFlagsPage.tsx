import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { createApiClient } from "../api";

interface FeatureFlag {
    id: string;
    flagId: string;
    companyId: string;
    serviceId: string;
    type: string;
    defaultValue: boolean;
}

export default function FeatureFlagsPage() {
    const { instance } = useMsal();
    const { companyId, serviceId } = useParams<{ companyId: string; serviceId: string }>();
    const navigate = useNavigate();
    const [flags, setFlags] = useState<FeatureFlag[]>([]);
    const [newFlagId, setNewFlagId] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const fetchFlags = async () => {
        try {
            const api = await createApiClient(instance);
            const response = await api.get(`/api/featureflag/service/${serviceId}`);
            setFlags(response.data);
        } catch {
            setFlags([]);
        } finally {
            setLoading(false);
        }
    };

    const createFlag = async () => {
        if (!newFlagId.trim()) return;
        try {
            const api = await createApiClient(instance);
            await api.post("/api/featureflag", {
                flagId: newFlagId,
                companyId,
                serviceId,
                type: "boolean",
                defaultValue: false,
            });
            setNewFlagId("");
            fetchFlags();
        } catch {
            setError("Failed to create feature flag.");
        }
    };

    const toggleFlag = async (flag: FeatureFlag) => {
        try {
            const api = await createApiClient(instance);
            await api.put(`/api/featureflag/${flag.id}`, {
                ...flag,
                defaultValue: !flag.defaultValue,
            });
            fetchFlags();
        } catch {
            setError("Failed to update feature flag.");
        }
    };

    const deleteFlag = async (flag: FeatureFlag) => {
        try {
            const api = await createApiClient(instance);
            await api.delete(`/api/featureflag/${flag.id}?serviceId=${flag.serviceId}`);
            fetchFlags();
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete feature flag.");
        }
    };

    useEffect(() => { fetchFlags(); }, []);

    if (loading) return <p>Loading...</p>;

    return (
        <div>
            <button className="back-btn" onClick={() => navigate(`/companies/${companyId}/services`)}>← Back to Services</button>
            <h2>Feature Flags</h2>
            {error && <p className="error">{error}</p>}
            <div className="add-form">
                <input
                    type="text"
                    placeholder="Flag name (e.g. dark-mode)"
                    value={newFlagId}
                    onChange={e => setNewFlagId(e.target.value)}
                />
                <button onClick={createFlag}>Add Flag</button>
            </div>
            <ul className="list">
                {flags.map(flag => (
                    <li key={flag.id} className="list-item">
                        <span className="flag-name">{flag.flagId}</span>
                        <label className="toggle">
                            <input
                                type="checkbox"
                                checked={flag.defaultValue}
                                onChange={() => toggleFlag(flag)}
                            />
                            <span className="toggle-slider"></span>
                        </label>
                        <span className={`flag-status ${flag.defaultValue ? "on" : "off"}`}>
                            {flag.defaultValue ? "ON" : "OFF"}
                        </span>
                        <button className="delete-btn" onClick={() => deleteFlag(flag)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
