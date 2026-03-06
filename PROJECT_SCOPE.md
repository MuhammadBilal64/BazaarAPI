## E-Commerce Backend API – Project Scope & Requirements

This project is a **learning / revision** playground for ASP.NET Core Web API concepts, using a simple e‑commerce domain (products, orders, users). The goal is not to build a production‑grade system, but to systematically practice common backend patterns and features.

---

## 1. High-Level Goals

- **Concept Revision**: Practice core ASP.NET Core Web API concepts end‑to‑end.
- **Clean Structure**: Organize code in a way that reflects good layering and separation of concerns.
- **Feature Coverage**: Touch as many realistic backend features as reasonable for a small project.
- **Testing Ground**: Have endpoints you can easily hit with Swagger, Postman, etc. to experiment.

---

## 2. Domain Overview

- **Users / Auth**
  - Customers (place orders).
  - Admins (manage catalog, view all orders).

- **Catalog**
  - Categories.
  - Products (basic info, price, stock).

- **Orders**
  - Cart / line items.
  - Order lifecycle (Created → Paid → Shipped → Cancelled).

- **Payments (simplified)**
  - Fake payment processing endpoint to simulate external integration.

---

## 3. Core Functional Requirements

### 3.1 User & Authentication

- **User registration & login**
  - Register new users with email, password, name.
  - Login to obtain a **JWT** access token.
  - Password hashing + basic validation.

- **Roles & Authorization**
  - At least two roles: `Customer`, `Admin`.
  - Protect admin‑only endpoints (e.g., managing products).
  - Regular customers can only see / modify **their own** orders.

### 3.2 Product & Catalog Management

- **Public Catalog**
  - List products with:
    - Pagination (page, pageSize).
    - Filtering (by category, price range, in‑stock only).
    - Sorting (price, newest).
  - Get product by id.

- **Admin Catalog Management**
  - Create, update, delete products.
  - Create, update, delete categories.
  - Basic validation (required fields, price > 0, etc.).

### 3.3 Cart & Orders

- **Shopping Cart (per user)**
  - Add product to cart, change quantity, remove item.
  - Get current cart contents.

- **Order Placement**
  - Convert cart to order.
  - Validate stock before creating order.
  - Decrease stock on successful order (or simulate).

- **Order Management**
  - Customer:
    - List own orders.
    - View order details.
  - Admin:
    - List all orders (with filtering by status, date range).
    - Update order status (e.g., mark as Shipped / Cancelled).

### 3.4 Payments (Mocked)

- **Payment Intent / Checkout Simulation**
  - Endpoint to simulate taking payment (no real gateway).
  - Accepts order id, returns success/failure.
  - On success, mark order as Paid.

---

## 4. Web API Concepts to Practice

This project should intentionally include the following **technical concepts**:

### 4.1 Routing & Controllers

- Attribute routing (e.g., `[Route("api/[controller]")]`, `[HttpGet("{id}")]`).
- Route constraints and query string parameters.
- Versioning (e.g., `/api/v1/...` and preparing for `/api/v2/...`).

### 4.2 Model Binding & Validation

- Request/response DTOs (separate from domain entities).
- Data annotations (e.g., `[Required]`, `[Range]`, custom validation).
- Fluent validation or custom validation logic where helpful.
- Proper use of `ModelState` and unified validation error responses.

### 4.3 Error Handling & Problem Details

- Global exception handling middleware.
- Consistent error responses (e.g., RFC 7807 style problem details).
- Handling:
  - 400 (validation).
  - 401/403 (auth/authorization).
  - 404 (missing resources).
  - 409 (conflicts, e.g., stock issues).

### 4.4 Logging & Monitoring

- Use ASP.NET Core logging abstractions.
- Log key events (auth failures, order created, payment failure).
- Correlation IDs or request IDs for tracing (optional but good practice).

### 4.5 Persistence & Data Access

- Use **Entity Framework Core** (or similar) with:
  - Code‑first migrations.
  - Entities for User, Product, Category, CartItem, Order, OrderItem, etc.
  - Relationships (one‑to‑many, many‑to‑one).
- Repository or service layer to keep controllers thin.
- Basic seeding of sample data for testing.

### 4.6 Security Practices

- Authentication with JWT bearer tokens.
- Authorization with role‑based policies.
- Protect sensitive endpoints with `[Authorize]` attributes.
- Basic input validation to avoid obvious issues.

### 4.7 Cross-Cutting Concerns

- **DTO mapping** (manual or using a mapper library).
- **Pagination pattern** (standard response shape with totalCount, page, pageSize).
- **Filtering/sorting conventions** (query parameters).
- **Response wrapping** (optional): consistent envelope for responses.

### 4.8 API Documentation

- Use **Swagger / Swashbuckle**:
  - Add XML comments on controllers and models.
  - Group endpoints logically (e.g., Catalog, Orders, Auth).
  - Define reusable response types where useful.

### 4.9 Caching

- Start with **in-memory caching**:
  - Use `IMemoryCache` for frequently-read data (e.g., product list, categories).
  - Configure **response caching middleware** for simple GET endpoints.
- Then learn **distributed caching with Redis** (new for you):
  - Configure Redis as a distributed cache provider.
  - Cache product listings and/or category data in Redis.
  - Understand cache invalidation when products/categories change.

---

## 5. Non-Functional Requirements

- **Code Organization**
  - Clear separation into layers/namespaces:
    - API (controllers).
    - Application / Services.
    - Domain/Entities.
    - Infrastructure / Data access.

- **Performance / UX (API side)**
  - Use async/await for I/O bound code.
  - Consider simple caching for frequently-read data (e.g., product list).

- **Testing**
  - Unit tests for core services (e.g., order creation logic).
  - At least a few integration tests for API endpoints.

---

## 6. Stretch Goals (Optional Extras)

Use these only if you want to revise even more concepts later:

- **Refresh tokens** and token rotation.
- **Soft deletes** for products.
- **Background jobs** (e.g., send order confirmation emails using a queue).
- **Rate limiting / throttling** for public endpoints.
- **Multi-tenancy** simulation (e.g., multiple stores).

---

## 7. Suggested Implementation Order

1. **Set up basic project** (done: controllers + Swagger).
2. **Define domain models and DbContext**.
3. **Implement product catalog (read‑only)** with pagination/filtering.
4. **Add JWT auth + roles** (register/login).
5. **Protect admin endpoints** and add product/category management.
6. **Implement cart and basic order creation**.
7. **Add mock payment flow** and order status updates.
8. **Add error handling middleware & logging improvements**.
9. **Write some tests** and refine documentation.

This file is your **guide** for what to build next. As you learn and revise new topics, feel free to update this scope to add or adjust requirements.

