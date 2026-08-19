# 6. Matriz de Transição de Setores

Esta matriz define quais setores de destino estão disponíveis ao despachar uma OS a partir do setor atual:

| Setor de Origem | Destinos Permitidos | Quem pode executar? |
| :--- | :--- | :--- |
| **Vendas (Entrada)** | Criação, Recorte, Impressão, Preparação, Frota, Acabamento | Vendedor |
| **Criação** | Recorte, Impressão, Preparação, Frota, Acabamento | Operacional (Criação) |
| **Impressão** | Recorte, Preparação, Frota, Acabamento | Operacional (Impressão) |
| **Recorte** | Preparação, Frota, Acabamento | Operacional (Recorte) |
| **Preparação** | Frota | Operacional (Preparação) |
| **Acabamento** | Prateleira | Operacional (Acabamento) |
| **Frota** | Pátio | Operacional (Frota) |
| **Prateleira** | Financeiro | Vendedor / Diretoria |
| **Pátio** | Financeiro | Vendedor / Diretoria |

> **Regra de Exceção (Retorno):** Adicionalmente aos destinos acima, qualquer setor pode despachar a OS de volta **exclusivamente para o setor de onde ela veio**.
