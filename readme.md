# Sistema de Ordens de Serviço & Analytics API 🛠️📊

API REST desenvolvida em **.NET 10** focada no gerenciamento, rastreamento imutável de movimentações e extração de métricas operacionais de Ordens de Serviço (OS). 

Projetada para atender cenários locais de empresas de pequeno e médio porte, fornecendo desacoplamento completo para consumo de dados via ferramentas de Business Intelligence (Power BI, Metabase, Excel).

---

## 🏗️ Arquitetura e Tecnologias

- **Linguagem & Framework:** C# / .NET 10
- **Persistência de Dados:** Entity Framework Core (EF Core) + SQLite
- **Testes Automatizados:** xUnit, Moq e FluentAssertions
- **Padrões & Boas Práticas:**
  - **Domain-Driven Design (DDD Leve):** Regras de negócio encapsuladas no domínio.
  - **DTO Pattern:** Mapeamento de entradas e saídas desacoplado do banco relacional.
  - **Global Error Handling:** Middleware centralizado de exceções (`RFC 7807`).
  - **BI Integration Layer:** Leitura sem travamento (*AsNoTracking*) para pipelines de BI.

---

## 🗄️ Estrutura do Banco de Dados & Histórico

O sistema utiliza o rastreamento imutável de status. Cada alteração gera um evento registrado com data/hora e o funcionário responsável:

```text
[ OrdemServico ] 1 ─── N [ HistoricoStatus ]