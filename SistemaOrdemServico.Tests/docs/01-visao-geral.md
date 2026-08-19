# 1. Visão Geral do Projeto

## 1.1 Problema de Negócio
No setor de Comunicação Visual, os processos produtivos passam por múltiplas etapas (criação, impressão, corte, acabamento, frota, etc.). A ausência de rastreabilidade precisa gera problemas como:
* Dificuldade em saber a localização exata de uma Ordem de Serviço (OS).
* Mistura indevida entre o tempo que a OS ficou aguardando na fila e o tempo efetivo de trabalho.
* Dependência de comunicação verbal/informal para localizar pedidos.
* Impossibilidade de identificar gargalos e causas de atraso de forma auditável.

## 1.2 Propósito do Sistema
O **OS Tracker** atua como uma camada enxuta de coleta de eventos operacionais. Ele registra com precisão cada movimentação entre setores, construindo um histórico confiável sem criar sobrecarga burocrática para a operação.

## 1.3 Premissas e Limites
* **O ERP é o sistema oficial de vendas/OS:** O OS Tracker não substitui o ERP nem emite faturamento. O número da OS é informado pelo vendedor ao registrar o fluxo.
* **Eventos legíveis:** Cada ação no sistema (Criar, Receber, Despachar, Cancelar) gera um registro imutável no banco de dados.
