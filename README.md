# dioflix

Gerenciador de Catálogos tipo Netflix com Azure Functions e Cosmos DB.

- Documentação de arquitetura: `docs/arquitetura.md`
- Página de teste: `consulta-cards.html` (exemplo de consumo das APIs)

## Endpoints (dev)

- Upload para Storage (Blob): `POST /api/dataStorage`
- Criar/atualizar filme (Cosmos DB): `POST /api/movie`
- Detalhe por Id: `GET /api/detail?id=...`
- Listar todos: `GET /api/all`

Exemplos de cURL estão no arquivo `docs/arquitetura.md`.