import React, { useState } from 'react';
import axios from 'axios';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { Button, Box, Typography } from '@mui/material';

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

  const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null);
  };

  const upload = async () => {
    if (!file) return alert('Select file');
    setLoading(true);
    try {
      const fd = new FormData();
      fd.append('file', file);
      const resp = await axios.post<PairResultVM[]>('/api/employee/upload', fd, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      console.log('Response data:', resp.data);
      
      // Handle if response is a single object or array
      const results = Array.isArray(resp.data) ? resp.data : [resp.data];
      
      // flatten to grid rows: each project per pair
      const data = results.flatMap(pr => pr.projects.map(p => ({
        employeeId1: pr.employeeIdA,
        employeeId2: pr.employeeIdB,
        projectId: p.projectId,
        daysWorked: p.daysWorked
      })));
      console.log('Mapped data:', data);
      // add id for DataGrid
      const withId = data.map((r, idx) => ({ id: idx+1, ...r }));
      console.log('Final rows with IDs:', withId);
      setRows(withId);
    } catch (err) {
      console.error(err);
      alert('Upload failed');
    } finally {
      setLoading(false);
    }
  };

  const columns: GridColDef[] = [
    { field: 'employeeId1', headerName: 'Employee #1', width: 130 },
    { field: 'employeeId2', headerName: 'Employee #2', width: 130 },
    { field: 'projectId', headerName: 'Project ID', width: 120 },
    { field: 'daysWorked', headerName: 'Days Worked', width: 130 }
  ];

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h6">Upload CSV</Typography>
      <input type="file" accept=".csv" onChange={onFileChange} />
      <Button variant="contained" onClick={upload} disabled={loading || !file} sx={{ ml: 2 }}>
        {loading ? 'Uploading...' : 'Upload'}
      </Button>

      <Box sx={{ height: 500, mt: 3 }}>
        <DataGrid 
          rows={rows} 
          columns={columns} 
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[25, 50, 100]}
        />
      </Box>
    </Box>
  );
}
