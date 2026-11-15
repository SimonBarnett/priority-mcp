/**
 * Plain-English “trained on” / catalog-scope copy for the mcp home page (FR #608).
 * Keep free of secrets; readable to a non-agent human.
 */

export const TRAINING_EXPLAINER_TITLE = 'What the agent has been trained on';

export const TRAINING_EXPLAINER_BODY =
  'The catalog is a library of repeatable Priority procedures harvested from Medatech-network lessons and field playbooks — form preparation, procedure prep, OData, hours, backups, service-desk fixes, UAT smokes, and more. Each skill is a focused recipe with hard rules (allowlisted instances, SQL gates, no invented WCF URLs). The agent selects and follows these skills on your side after you connect your instances.';

export const TRAINING_EXPLAINER_POINTS = Object.freeze([
  'Grab skill markdown / JSON from this catalog (MCP tools).',
  'Execute locally against ERP instances you allowlisted on your machine.',
  'This public host does not compile forms, cannot see on-prem SQL, and never receives your passwords.',
]);

/** Phrases acceptance tests pin (issue #608 testable criteria). */
export const TRAINING_EXPLAINER_REQUIRED_PHRASES = Object.freeze([
  'Medatech',
  'allowlisted',
  'does not compile forms',
  'on-prem SQL',
]);

export function trainingExplainerText() {
  return [TRAINING_EXPLAINER_TITLE, TRAINING_EXPLAINER_BODY, ...TRAINING_EXPLAINER_POINTS].join(
    '\n',
  );
}
