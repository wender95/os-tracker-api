# 7. KPIs e Camada de Analytics

O OS Tracker armazena eventos brutos (`id`, `fluxo_id`, `setor_id`, `tipo_evento`, `usuario_id`, `data_hora`). A camada de Analytics (BI) deriva os seguintes indicadores de desempenho:

## 1. Tempo de Espera (Queue Time)
Tempo de Espera = Data/Hora do Recebimento no Setor B - Data/Hora do Despacho do Setor A
* **Objetivo:** Medir gargalos e acúmulo de trabalho entre setores.

## 2. Tempo de Processamento (Touch Time)
Tempo de Processamento = Data/Hora do Despacho no Setor A - Data/Hora do Recebimento no Setor A
* **Objetivo:** Medir a produtividade efetiva em cada etapa produtiva.

## 3. Lead Time Total da OS
Lead Time = Data/Hora de Encerramento (Financeiro) - Data/Hora de Criação (Vendas)

## 4. Taxa de Retrabalho (Loop Back Ratio)
* Contagem do número de vezes que um evento de despacho retornou para um setor anterior (ex: Impressão → Criação).
