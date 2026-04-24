import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { createApiClient } from "../api";

interface Service {
    id: string;
    serviceName: string;
    description: string;
}

function avatarColor(name: string) {
    const index = name.charCodeAt(0) % 8;
    return `color-${index}`;
}

export default function ServicesPage() {
    const { instance } = useMsal();
    const { companyId } = useParams<{ companyId: string }>();
    const navigate = useNavigate();
    const [services, setServices] = useState<Service[]>([]);
    const [companyName, setCompanyName] = useState("");
    const [flagCounts, setFlagCounts] = useState<Record<string, number>>({});
    const [newName, setNewName] = useState("");
    const [newDesc, setNewDesc] = useState("");
    const [newServiceId, setNewServiceId] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const fetchCompanyName = async () => {
        try {
            const api = await createApiClient(instance);
            const response = await api.get(`/api/company/${companyId}`);
            setCompanyName(response.data.name);
        } catch {
            // company name is optional, ignore errors
        }
    };

    const fetchServices = async () => {
        try {
            const api = await createApiClient(instance);
            const response = await api.get(`/api/service/company/${companyId}`);
            const data: Service[] = response.data;
            setServices(data);

            const counts: Record<string, number> = {};
            await Promise.all(data.map(async service => {
                try {
                    const res = await api.get(`/api/featureflag/service/${service.id}`);
                    counts[service.id] = res.data.length;
                } catch {
                    counts[service.id] = 0;
                }
            }));
            setFlagCounts(counts);
        } catch {
            setServices([]);
        } finally {
            setLoading(false);
        }
    };

    const createService = async () => {
        if (!newName.trim()) return;
        try {
            const api = await createApiClient(instance);
            await api.post("/api/service", { id: newServiceId || undefined, serviceName: newName, description: newDesc, companyId });
            setNewName("");
            setNewDesc("");
            setNewServiceId("");
            fetchServices();
        } catch {
            setError("Failed to create service.");
        }
    };

    const deleteService = async (id: string) => {
        try {
            const api = await createApiClient(instance);
            await api.delete(`/api/service/${id}`);
            fetchServices();
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete service.");
        }
    };

    useEffect(() => {
        fetchCompanyName();
        fetchServices();
    }, []);

    if (loading) return <p>Loading...</p>;

    return (
        <div>
            <nav className="breadcrumb">
                <span className="breadcrumb-current">{companyName || "Services"}</span>
            </nav>
            <h2>Services</h2>
            {error && <p className="error">{error}</p>}
            <div className="add-form">
                <input
                    type="text"
                    placeholder="Service name"
                    value={newName}
                    onChange={e => setNewName(e.target.value)}
                />
                <input
                    type="text"
                    placeholder="Description"
                    value={newDesc}
                    onChange={e => setNewDesc(e.target.value)}
                />
                <input
                    type="text"
                    placeholder="Service ID (optional — use Azure AD app client ID)"
                    value={newServiceId}
                    onChange={e => setNewServiceId(e.target.value)}
                />
                <button onClick={createService}>Add Service</button>
            </div>
            <ul className="list">
                {services.map(service => (
                    <li key={service.id} className="list-item">
                        <span className={`item-avatar ${avatarColor(service.serviceName)}`}>{service.serviceName.charAt(0).toUpperCase()}</span>
                        <span onClick={() => navigate(`/companies/${companyId}/services/${service.id}/flags`)} className="link">
                            {service.serviceName}
                        </span>
                        {service.description && <span className="item-description">{service.description}</span>}
                        <span className="count-badge">{flagCounts[service.id] ?? 0} {flagCounts[service.id] === 1 ? "flag" : "flags"}</span>
                        <button className="delete-btn" onClick={() => deleteService(service.id)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
