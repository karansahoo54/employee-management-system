# Enterprise Employee Management System (EMS)

A modern, full-stack, enterprise-grade Employee Management System built with **ASP.NET Core 8 Web API** and **Angular 19**.

---

## 🌟 Tech Stack

- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core 8, Npgsql PostgreSQL Provider, QuestPDF, ClosedXML, BCrypt.Net, JWT Bearer Authentication.
- **Frontend**: Angular 19 (Standalone Components), RxJS, Bootstrap 5 & Bootstrap Icons, Modern CSS.
- **Database**: PostgreSQL (Supabase Free Cloud Tier / Managed DB).
- **Deployment**: 100% Free-of-cost cloud hosting architecture:
  - **Database**: Supabase PostgreSQL (500MB Free)
  - **Backend API**: Render.com Free Docker Web Service
  - **Frontend SPA**: Vercel / Netlify / Render Free Static Site

---

## 🚀 Pre-Configured Demo Credentials

The backend automatically seeds the database on startup with default roles, departments, and starter employees:

| Role | Username | Password |
| :--- | :--- | :--- |
| **Administrator** | `admin` | `Admin@123` |

*(You can also use the **"Fill Demo Admin Credentials"** button on the frontend login page)*

---

## 💻 Running Locally

### 1. Backend (`EMS.API`)
Requirements: .NET 8 SDK (or .NET 10 SDK)

```bash
cd EMS.API
dotnet run --project EMS.API\EMS.API.csproj --urls=http://localhost:5000
```
- **Swagger Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Health Check**: [http://localhost:5000/health](http://localhost:5000/health)

### 2. Frontend (`ems-frontend-complete`)
Requirements: Node.js 18+ and npm

```bash
cd ems-frontend-complete
npm install
npm start
```
Open your browser at [http://localhost:4200](http://localhost:4200).

---

## ☁️ 100% Free Live Cloud Deployment Guide

### Step 1: Database (Supabase PostgreSQL - 100% Free)
1. Go to [supabase.com](https://supabase.com) and create a free project.
2. In Project Settings -> Database -> Connection string, choose **Transaction pooler (Port 6543)**.
3. Your connection string format:
   `Host=aws-0-[region].pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.[project-id];Password=[your-password];SSL Mode=Require;Multiplexing=false;No Reset On Close=true`
*(Already configured and pre-tested in `appsettings.json`)*.

---

### Step 2: Backend Deployment (Render.com - 100% Free)
1. Push this repository to your GitHub account:
   ```bash
   git add .
   git commit -m "feat: complete debugging and free cloud live deployment configuration"
   git push origin main
   ```
2. Log into [render.com](https://render.com) (free account).
3. Click **New +** -> **Web Service** -> Link your GitHub repository.
4. Set the following settings:
   - **Root Directory**: `EMS.API`
   - **Environment / Runtime**: `Docker`
   - **Dockerfile Path**: `./Dockerfile`
   - **Plan**: `Free`
5. In **Environment Variables**, add:
   - `ConnectionStrings__DefaultConnection`: *(your Supabase connection string)*
   - `Jwt__Key`: `SuperSecretEmployeeManagementSystemJwtKey2026!MustBeAtLeast512BitsLongForSecurity#9876543210`
   - `Jwt__Issuer`: `EMS.API`
   - `Jwt__Audience`: `EMS.Client`
   - `ASPNETCORE_ENVIRONMENT`: `Production`
6. Click **Create Web Service**.
   Render will build the multi-stage Docker image and start the service (e.g. `https://your-ems-api.onrender.com`).
   On first launch, the API will automatically migrate the database and seed the `admin` account and sample employees!

---

### Step 3: Frontend Deployment (Vercel / Netlify - 100% Free)

#### Option A: Vercel (Recommended)
1. In `ems-frontend-complete/src/environments/environment.prod.ts`, update `apiUrl`:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://your-ems-api.onrender.com/api'
   };
   ```
2. Log into [vercel.com](https://vercel.com) and click **Add New Project** -> Import repository.
3. Configure project settings:
   - **Root Directory**: `ems-frontend-complete`
   - **Framework Preset**: `Angular`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist/ems-frontend-complete/browser`
4. Click **Deploy**.
   Vercel reads `vercel.json` and serves the Angular application with single-page app routing enabled.

#### Option B: Netlify
1. Log into [netlify.com](https://netlify.com) -> **Add new site** -> **Import an existing project**.
2. Settings:
   - **Base directory**: `ems-frontend-complete`
   - **Build command**: `npm run build`
   - **Publish directory**: `ems-frontend-complete/dist/ems-frontend-complete/browser`
3. Click **Deploy Site**. Netlify automatically uses `_redirects` for SPA routing.

---

## 🔒 Security & Architecture Features
- **BCrypt Password Hashing**: Passwords stored with salt and work factor.
- **512-Bit HMAC-SHA256 JWT Authentication**: Secure role-based tokens with automated client-side interception.
- **Enterprise Reporting**:
  - ClosedXML Excel exports for Employee Directory, Department Summary, and Attendance.
  - QuestPDF vector PDF generation for Employee Directory and Payroll Outlay.
- **Resilient Database Architecture**: EF Core connection pooling configured with retry-on-failure.
- **Automated Database Seeding**: Admin user, departmental hierarchy, and sample data auto-populated on first boot.
