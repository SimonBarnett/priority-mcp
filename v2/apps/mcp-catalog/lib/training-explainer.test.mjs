import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import {
  TRAINING_EXPLAINER_BODY,
  TRAINING_EXPLAINER_POINTS,
  TRAINING_EXPLAINER_REQUIRED_PHRASES,
  TRAINING_EXPLAINER_TITLE,
  trainingExplainerText,
} from './training-explainer.mjs';

describe('training explainer (FR #608)', () => {
  it('has a human-readable trained-on title', () => {
    assert.match(TRAINING_EXPLAINER_TITLE, /trained on/i);
  });

  it('mentions Medatech / Priority playbook scope', () => {
    assert.match(TRAINING_EXPLAINER_BODY, /Medatech/i);
    assert.match(TRAINING_EXPLAINER_BODY, /Priority/i);
    assert.match(TRAINING_EXPLAINER_BODY, /playbook/i);
  });

  it('pins allowlisted instances and host boundaries', () => {
    const text = trainingExplainerText();
    for (const phrase of TRAINING_EXPLAINER_REQUIRED_PHRASES) {
      assert.match(text, new RegExp(phrase.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'i'));
    }
  });

  it('lists grab / local execute / host isolation bullets', () => {
    assert.equal(TRAINING_EXPLAINER_POINTS.length, 3);
    assert.match(TRAINING_EXPLAINER_POINTS[0], /Grab/i);
    assert.match(TRAINING_EXPLAINER_POINTS[1], /allowlisted/i);
    assert.match(TRAINING_EXPLAINER_POINTS[2], /compile forms/i);
  });
});
