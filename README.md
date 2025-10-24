# 🛒 E-Commerce Web App

Proyecto full stack basado en **Clean Architecture** con **ASP.NET Core**, **SQL Server**, **TailwindCSS**, **TypeScript** y **Docker**.  
Diseñado para ser escalable, mantenible y de alto rendimiento, ideal para entornos de producción modernos.

---

## 🚀 Tecnologías principales

| Área | Tecnología |
|------|-------------|
| Backend | ASP.NET Core (.NET 8) |
| Base de Datos | SQL Server 2022 |
| ORM | Entity Framework Core |
| Frontend | HTML + TailwindCSS + TypeScript |
| Contenedores | Docker / Docker Compose |
| API | JSON REST |
| Autenticación | JWT + ASP.NET Identity |
| Testing | xUnit + Moq |
| Documentación | Swagger (OpenAPI) |

---

## 🧩 Arquitectura

El proyecto está estructurado bajo el patrón **Clean Architecture**, asegurando una separación clara entre capas:

- **Domain** → Entidades de negocio puras, sin dependencias externas.  
- **Application** → Casos de uso, lógica de negocio y validaciones.  
- **Infrastructure** → Acceso a datos, repositorios, servicios externos (EF Core, SQL Server).  
- **API** → Endpoints REST, controladores, autenticación y configuración.  
- **Frontend** → Interfaz visual (HTML + Tailwind + TS) que consume la API vía JSON.  

Esta estructura permite escalar el proyecto, mejorar mantenibilidad y realizar testing independiente por capa.

---

## ⚙️ Funcionalidades principales

- 🔐 Registro e inicio de sesión con JWT  
- 👤 Gestión de usuarios y roles (cliente / admin)  
- 🛍️ CRUD de productos  
- 📦 Carrito y órdenes de compra  
- 💾 Persistencia en SQL Server mediante EF Core  
- 📜 Documentación automática con Swagger  
- 🧱 Despliegue mediante Docker Compose (backend + DB + frontend)

---

## 🧠 Flujo general

1. El **frontend (HTML + TS)** consume la **API .NET Core** mediante `fetch()` o Axios.  
2. La **API** procesa las peticiones, valida datos y accede a la base de datos vía **EF Core**.  
3. La **base de datos SQL Server** almacena los datos de usuarios, productos y pedidos.  
4. Todo el entorno corre aislado en **contenedores Docker**.  

---

## 🔐 Autenticación y seguridad

- Sistema de autenticación con **JWT (JSON Web Tokens)**.  
- Roles y permisos manejados por **ASP.NET Identity**.  
- CORS configurado para permitir únicamente dominios autorizados del frontend.  
- Validaciones de entrada con **DataAnnotations** o **FluentValidation**.  
- Claves sensibles gestionadas por variables de entorno (`.env`) o **User Secrets**.  

---

