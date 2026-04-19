import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { createApiClient } from "../api";

interface Company {
    id: string;
    name: string;
}

export default function CompaniesPage() {
    const { instance } = useMsal();
    const navigate = useNavigate();
    const [companies, setCompanies] = useState<Company[]>([]);
    const [serviceCounts, setServiceCounts] = useState<Record<string, number>>({});
    const [newName, setNewName] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const fetchCompanies = async () => {
        try {
            const api = await createApiClient(instance);
            const response = await api.get("/api/company");
            const data: Company[] = response.data;
            setCompanies(data);

            const counts: Record<string, number> = {};
            await Promise.all(data.map(async company => {
                try {
                    const res = await api.get(`/api/service/company/${company.id}`);
                    counts[company.id] = res.data.length;
                } catch {
                    counts[company.id] = 0;
                }
            }));
            setServiceCounts(counts);
        } catch {
            setError("Failed to load companies.");
        } finally {
            setLoading(false);
        }
    };

    const createCompany = async () => {
        if (!newName.trim()) return;
        try {
            const api = await createApiClient(instance);
            await api.post("/api/company", { name: newName });
            setNewName("");
            fetchCompanies();
        } catch {
            setError("Failed to create company.");
        }
    };

    const deleteCompany = async (id: string) => {
        try {
            const api = await createApiClient(instance);
            await api.delete(`/api/company/${id}`);
            fetchCompanies();
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete company.");
        }
    };

    useEffect(() => { fetchCompanies(); }, []);

    if (loading) return <p>Loading...</p>;

    return (
        <div>
            <nav className="breadcrumb">
                <span className="breadcrumb-current">Companies</span>
            </nav>
            <h2>Companies</h2>
            {error && <p className="error">{error}</p>}
            <div className="add-form">
                <input
                    type="text"
                    placeholder="Company name"
                    value={newName}
                    onChange={e => setNewName(e.target.value)}
                />
                <button onClick={createCompany}>Add Company</button>
            </div>
            <ul className="list">
                {companies.map(company => (
                    <li key={company.id} className="list-item">
                        <span onClick={() => navigate(`/companies/${company.id}/services`)} className="link">
                            {company.name}
                        </span>
                        <span className="count-badge">{serviceCounts[company.id] ?? 0} {serviceCounts[company.id] === 1 ? "service" : "services"}</span>
                        <button className="delete-btn" onClick={() => deleteCompany(company.id)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}
