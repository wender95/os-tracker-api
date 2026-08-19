# 2. Análise do Processo Operacional

## 2.1 Visão Geral do Ciclo de Produção
O fluxo produtivo abrange desde a entrada comercial até a destinação final:
1. **Comercial / Vendas:** Abre o fluxo no OS Tracker informando o número da OS do ERP e direcionando para os setores iniciais.
2. **Criação:** Desenvolvimento e aprovação de arte gráfica.
3. **Impressão & Recorte:** Preparação dos materiais gráficos (impressão digital, corte especial, faca).
4. **Preparação:** Encaminha obrigatoriamente para a **Frota**.
5. **Acabamento:** Encaminha obrigatoriamente para a **Prateleira**.
6. **Frota / Instalação:** Envelopamento de veículos e serviços de campo. Encaminha obrigatoriamente para o **Pátio**.
7. **Destinação Física Intermediária:**
   * `PRATELEIRA`: Serviços pequenos/médios aguardando liberação.
   * `PATIO`: Veículos ou estruturas de grande porte aguardando liberação.
8. **Entrega & Financeiro:** Vendedor ou Diretoria realiza a saída das áreas físicas (`PRATELEIRA` ou `PATIO`) e despacha para o **Financeiro**, momento em que o fluxo produtivo é encerrado no OS Tracker.

## 2.2 Estrutura de Múltiplos Fluxos Paralelos (Split)
Uma única OS pode conter itens independentes. Exemplo:
* **OS 5050:**
  * **Fluxo A:** Lona impressa → Impressão → Acabamento → Prateleira.
  * **Fluxo B:** Adesivação de frota → Frota → Pátio.

O OS Tracker trata cada fluxo de forma autônoma.
