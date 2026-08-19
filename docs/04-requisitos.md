# 4. Requisitos do Sistema

## 4.1 Requisitos Funcionais (RF)
### RF01 - Abertura e Criação de Fluxos de OS
* O sistema deve permitir que o usuário com perfil **Vendedor** crie novos fluxos para uma Ordem de Serviço informando o número da OS (gerado externamente pelo ERP).
* O sistema deve permitir que um mesmo número de OS possua múltiplos fluxos paralelos e independentes.
* O sistema deve permitir que o Vendedor selecione o setor inicial válido para cada fluxo criado (ex: Criação, Recorte, Impressão, Preparação, Frota ou Acabamento).

### RF02 - Gestão e Fila de Trabalhos Operacionais
* O sistema deve exibir a fila de trabalho contendo apenas as OS/fluxos destinados ao setor do usuário logado (para perfil **Operacional**).
* O sistema deve permitir que qualquer operador do setor realize a ação de **Receber** uma OS/fluxo que esteja em sua fila.
* O sistema deve gravar no banco de dados a data, hora e o ID do primeiro usuário que clicou em **Receber** para aquele setor.

### RF03 - Despacho e Transição de Setores
* O sistema deve permitir que o usuário despache uma OS/fluxo para o próximo setor após o recebimento.
* O sistema deve restringir as opções do setor de destino com base nas regras estabelecidas pela **Matriz de Transição**.
* O sistema deve permitir a opção de retorno da OS/fluxo **exclusivamente para o setor de origem imediata** (setor que a despachou anteriormente).

### RF04 - Destinação Física Intermediária e Saída para Financeiro
* O sistema deve exigir o direcionamento exclusivo para `PRATELEIRA` ao despachar do setor de Acabamento.
* O sistema deve exigir o direcionamento exclusivo para `PATIO` ao despachar do setor de Frota.
* O sistema deve restringir aos perfis **Vendedor** e **Diretoria** a permissão de dar saída dos locais físicos (`PRATELEIRA` ou `PATIO`) encaminhando a OS para o `FINANCEIRO`.
* O sistema deve marcar o fluxo como **Encerrado** assim que for despachado para o setor Financeiro.

### RF05 - Consulta e Visualização de OS
* O sistema deve permitir que os perfis **Vendedor** e **Diretoria** (incluindo Financeiro e RH) consultem e visualizem o histórico/status de qualquer OS no sistema, independentemente de quem a criou.
* O sistema deve restringir a consulta do perfil **Operacional** apenas às OS e fluxos associados ao seu setor de atuação.
* O sistema deve exibir uma linha do tempo (timeline) simples e operacional com todos os eventos gravados (Criada, Recebida, Despachada, Cancelada) para cada fluxo da OS.

### RF06 - Cancelamento de OS
* O sistema deve permitir que apenas os perfis **Vendedor** e **Diretoria** cancelem uma OS ou fluxo ativo.
* O sistema deve obrigatoriamente solicitar e registrar uma justificativa/motivo no momento do cancelamento.
* O cancelamento deve alterar o status do fluxo para `CANCELADA`, mantendo todo o histórico de eventos imutável para auditoria.

### RF07 - Gestão de Acessos e Configurações (Admin)
* O sistema deve permitir que o **Administrador** cadastre, edite e inative usuários, vinculando-os aos seus respectivos perfis e setores.
* O sistema deve permitir a parametrização dos setores e das permissões da Matriz de Transição.

## 4.2 Requisitos Não-Funcionais (RNF)
### RNF01 - Imutabilidade e Auditoria de Eventos
* Toda ação de movimentação (Criação, Recebimento, Despacho, Cancelamento) deve gerar um registro imutável de evento no banco de dados contendo `id`, `fluxo_id`, `setor_id`, `tipo_evento`, `usuario_id` e `timestamp` (data/hora exata).
* Registros de eventos passados não podem ser alterados nem excluídos via aplicação.

### RNF02 - Desempenho e Disponibilidade
* O tempo de resposta para ações transacionais (cliques em "Receber" e "Despachar") deve ser inferior a 2 segundos em condições normais de rede.
* As telas de fila operacional devem ser leves, garantindo carregamento rápido mesmo em dispositivos móveis ou computadores de baixa performance no chão de fábrica.

### RNF03 - Interface e Usabilidade
* A interface do usuário operacional deve ser enxuta e focar na redução de cliques (foco na ação rápida: Receber -> Despachar).
* O design deve ser responsivo, adaptando-se a telas de computadores, tablets e smartphones.

### RNF04 - Arquitetura e Decoupling (Desacoplamento)
* O sistema deve ser estruturado em arquitetura REST API (Backend separado do Frontend Web).
* O banco de dados deve ser modelado de forma a servir de fonte bruta de dados para posterior integração com ferramentas de Business Intelligence (Power BI, Metabase, Grafana).

### RNF05 - Segurança e Autenticação
* O acesso ao sistema deve ser protegido por autenticação (usuário e senha ou token seguro).
* O sistema deve validar todas as permissões de acesso e movimentação no lado do servidor (Backend) e não apenas na interface gráfica (Frontend).
