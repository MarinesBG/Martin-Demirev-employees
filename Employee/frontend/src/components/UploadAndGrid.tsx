import React, { useState, useEffect, useMemo, useCallback } from 'react';
import axios from 'axios';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './UploadAndGrid.css';

type PairProject = {
  employeeId1: number;
  employeeId2: number;
  projectId: number;
  daysWorked: number;
};  

type PairResultVM = {
  employeeIdA: number;
  employeeIdB: number;
  totalDays: number;
  projects: PairProject[];
};

export default function UploadAndGrid() {
  const [file, setFile] = useState<File | null>(null);
  const [rows, setRows] = useState<PairProject[]>([]);
  const [loading, setLoading] = useState(false);
  const [paginationModel, setPaginationModel] = useState({ pageSize: 25, page: 0 });
  const [sortBy, setSortBy] = useState<'days' | 'project'>('days');

  // useEffect: File validation
  useEffect(() => {
    if (!file) return;

    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['text/csv', 'application/vnd.ms-excel'];

    if (file.size > maxSize) {
      toast.error('File size exceeds 10MB limit');
      setFile(null);
      return;
    }

    if (!allowedTypes.includes(file.type) && !file.name.endsWith('.csv')) {
      toast.error('Only CSV files are allowed');
      setFile(null);
      return;
    }

    toast.info(`File "${file.name}" selected (${(file.size / 1024).toFixed(2)} KB)`);
  }, [file]);

  // useCallback: Memoized event handlers
  const onFileChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null);
  }, []);

  const upload = useCallback(async () => {
    if (!file) {
      toast.warning('Please select a file first');
      return;
    }

    setLoading(true);
    const loadingToast = toast.loading('Uploading and processing file...');

    try {
      const fd = new FormData();
      fd.append('file', file);
      const resp = await axios.post<PairResultVM[]>('/api/employee/upload', fd, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      
      // Handle if response is a single object or array
      const results = Array.isArray(resp.data) ? resp.data : [resp.data];
      
      // flatten to grid rows: each project per pair
      const data = results.flatMap(pr => pr.projects.map(p => ({
        employeeId1: pr.employeeIdA,
        employeeId2: pr.employeeIdB,
        projectId: p.projectId,
        daysWorked: p.daysWorked
      })));

      // add id for DataGrid
      const withId = data.map((r, idx) => ({ id: idx+1, ...r }));
      setRows(withId);

      toast.update(loadingToast, {
        render: `Success! Found ${withId.length} collaboration record(s)`,
        type: 'success',
        isLoading: false,
        autoClose: 3000
      });
    } catch (err) {
      console.error(err);
      const errorMessage = axios.isAxiosError(err) 
        ? err.response?.data?.message || err.message 
        : 'An unexpected error occurred';
      
      toast.update(loadingToast, {
        render: `Upload failed: ${errorMessage}`,
        type: 'error',
        isLoading: false,
        autoClose: 5000
      });
    } finally {
      setLoading(false);
    }
  }, [file]);

  // useMemo: Sorted and filtered rows
  const processedRows = useMemo(() => {
    if (rows.length === 0) return [];

    // Sort rows based on selected criteria
    const sorted = [...rows].sort((a, b) => {
      if (sortBy === 'days') {
        return b.daysWorked - a.daysWorked; // Descending order
      } else {
        return a.projectId - b.projectId; // Ascending order
      }
    });

    return sorted;
  }, [rows, sortBy]);

  const columns: GridColDef[] = [
    { 
      field: 'employeeId1', 
      headerName: 'Employee #1', 
      flex: 1,
      minWidth: 150,
      headerAlign: 'center',
      align: 'center'
    },
    { 
      field: 'employeeId2', 
      headerName: 'Employee #2', 
      flex: 1,
      minWidth: 150,
      headerAlign: 'center',
      align: 'center'
    },
    { 
      field: 'projectId', 
      headerName: 'Project ID', 
      flex: 1,
      minWidth: 150,
      headerAlign: 'center',
      align: 'center'
    },
    { 
      field: 'daysWorked', 
      headerName: 'Days Worked Together', 
      flex: 1,
      minWidth: 180,
      headerAlign: 'center',
      align: 'center',
      renderCell: (params) => (
        <div className="days-cell">
          {params.value} days
        </div>
      )
    }
  ];

  return (
    <div className="upload-container">
      <ToastContainer 
        position="top-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="colored"
      />
      
      <div className="header-card">
        <h1 className="header-title">Employee Pair Analysis</h1>
        <p className="header-subtitle">
          Upload a CSV file to analyze employee collaboration patterns
        </p>
        
        <div className="upload-controls">
          <label className="upload-button">
            <CloudUploadIcon />
            Choose File
            <input 
              type="file" 
              accept=".csv" 
              onChange={onFileChange}
              className="visually-hidden"
            />
          </label>
          
          {file && (
            <span className="file-name">
              Selected: {file.name}
            </span>
          )}
          
          <button 
            className="submit-button"
            onClick={upload} 
            disabled={loading || !file}
          >
            {loading ? 'Processing...' : 'Upload & Analyze'}
          </button>
        </div>
      </div>

      {processedRows.length > 0 && (
        <div className="results-card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h2 className="results-title">Collaboration Results ({processedRows.length} records)</h2>
            <div>
              <label style={{ marginRight: '0.5rem', fontWeight: 500 }}>Sort by:</label>
              <select 
                value={sortBy} 
                onChange={(e) => setSortBy(e.target.value as 'days' | 'project')}
                style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #ddd' }}
              >
                <option value="days">Days Worked (High to Low)</option>
                <option value="project">Project ID (Low to High)</option>
              </select>
            </div>
          </div>
          <div className="grid-container custom-datagrid">
            <DataGrid 
              rows={processedRows} 
              columns={columns} 
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[25, 50, 100]}
            />
          </div>
        </div>
      )}
    </div>
  );
}
