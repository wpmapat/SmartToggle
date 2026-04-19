import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { createApiClient } from "../api";

interface Service {
    id: string;
    serviceName: string;
    description: string;
}

export default function ServicesPage() {
    const { instance } = useMsal();
    const { companyId } = useParams<{ companyId: string }>();
    const navigate = useNavigate();
    const [services, setServices] = useState<Service[]>([]);
    const [companyName, setCompanyName] = useState("");
    const [newName, setNewName] = useState("");
    const [newDesc, setNewDesc] = useState("");
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
            setServices(response.data);
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
            await api.post("/api/service", { serviceName: newName, description: newDesc, companyId });
            setNewName("");
            setNewDesc("");
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
            <button className="back-btn" onClick={() => navigate("/companies")}>← Back to Companies</button>
            <h2>Services {companyName && <span className="sub-heading">— {companyName}</span>}</h2>
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
                <button onClick={createService}>Add Service</button>
            </div>
            <ul className="list">
                {services.map(service => (
                    <li key={service.id} className="list-item">
                        <span onClick={() => navigate(`/companies/${companyId}/services/${service.id}/flags`)} className="link">
                            {service.serviceName}
                        </span>
                        {service.description && <span className="item-description">{service.description}</span>}
                        <button className="delete-btn" onClick={() => deleteService(service.id)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
