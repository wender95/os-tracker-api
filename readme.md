# OS Tracker - API REST 🛠️📊

API REST desenvolvida em **.NET 10** para rastreamento de eventos operacionais e movimentação de Ordens de Serviço (OS) em ambiente fabril/comercial.

---

## 📚 Documentação do Projeto

A especificação completa das regras de negócio, matriz de transição e arquitetura está organizada na pasta [`/docs`](./docs):

- [01. Visão Geral do Projeto](./docs/01-visao-geral.md)
- [02. Análise do Processo Operacional](./docs/02-analise-do-processo.md)
- [03. Stakeholders e Permissões](./docs/03-stakeholders-e-permissoes.md)
- [04. Requisitos do Sistema](./docs/04-requisitos.md)
- [05. Regras de Negócio](./docs/05-regras-de-negocio.md)
- [06. Matriz de Transição de Setores](./docs/06-fluxos-e-matriz-de-transicao.md)
- [07. KPIs e Camada de Analytics](./docs/07-kpis-e-analytics.md)
- [08. Roadmap](./docs/08-roadmap.md)

---

## 🏗️ Arquitetura e Tecnologias

- **Linguagem & Framework:** C# / .NET 10
- **Persistência:** Entity Framework Core + SQLite
- **Testes:** xUnit e Moq
- **Padrões:** Domain-Driven Design (DDD), DTO Pattern, Global Error Handling (`RFC 7807`)

---

## 🚀 Como Executar Localmente

```bash
cd SistemaOrdemServico
dotnet run --launch-profile httpcd SistemaOrdemServico.Tests