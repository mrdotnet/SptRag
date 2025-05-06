CREATE EXTENSION IF NOT EXISTS age;
LOAD 'age';
SET search_path = ag_catalog, "$user", public;

SELECT create_graph('lightrag_kg');

-- Example seed data (optional)
SELECT * FROM cypher('lightrag_kg', $$
  CREATE (:Entity {name: 'Azure AI', description: 'Microsoft Azure AI services'})
$$) AS (v agtype);
