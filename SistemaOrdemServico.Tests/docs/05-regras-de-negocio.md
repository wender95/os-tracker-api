# 5. Regras de Negócio (RN)

* **RN01 - Unicidade do Primeiro Recebimento:** O primeiro funcionário a clicar em "Receber" no setor é gravado como o responsável pelo início daquela etapa do fluxo.
* **RN02 - Preservação do Histórico de Cancelamento:** O cancelamento de uma OS altera seu status para `CANCELADA`, mas mantém salvos todos os eventos e movimentações passadas.
* **RN03 - Independência de Fluxos:** Uma OS com múltiplos fluxos (ex: Impressão e Frota) mantém cronogramas, responsáveis e status 100% independentes para cada fluxo.
* **RN04 - Destino Obrigatório por Escopo:** O operador não pode digitar o setor de destino livremente; deve escolher estritamente entre as opções validadas pela Matriz de Transição.
* **RN05 - Destinação Física Intermediária e Restrição de Saída:** Ao finalizar a etapa de Acabamento, a OS deve ir obrigatoriamente para `PRATELEIRA`. Ao finalizar a Frota, deve ir para `PATIO`. A saída de qualquer uma dessas áreas físicas para o `FINANCEIRO` é restrita aos perfis Vendedor e Diretoria.
* **RN06 - Sem Pausas Automáticas (MVP):** Não há estado "pausado" no MVP. O tempo entre o Recebimento e o Despacho é considerado tempo total no setor.
