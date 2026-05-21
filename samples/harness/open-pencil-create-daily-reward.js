const night = { type: 'SOLID', color: { r: 0.08, g: 0.06, b: 0.18 } };
const cardFill = { type: 'SOLID', color: { r: 0.98, g: 0.97, b: 0.94 } };
const gold = { type: 'SOLID', color: { r: 0.95, g: 0.72, b: 0.18 } };
const goldDark = { type: 'SOLID', color: { r: 0.82, g: 0.55, b: 0.08 } };
const ink = { type: 'SOLID', color: { r: 0.15, g: 0.12, b: 0.22 } };
const muted = { type: 'SOLID', color: { r: 0.45, g: 0.42, b: 0.52 } };
const claim = { type: 'SOLID', color: { r: 0.92, g: 0.38, b: 0.12 } };
const closeBg = { type: 'SOLID', color: { r: 0.88, g: 0.86, b: 0.9 } };
const white = { type: 'SOLID', color: { r: 1, g: 1, b: 1 } };
const coin = { type: 'SOLID', color: { r: 1, g: 0.92, b: 0.55 } };

function setupVerticalFrame(frame, spacing, padding) {
  frame.layoutMode = 'VERTICAL';
  frame.itemSpacing = spacing;
  frame.paddingTop = padding;
  frame.paddingRight = padding;
  frame.paddingBottom = padding;
  frame.paddingLeft = padding;
  frame.primaryAxisAlignItems = 'MIN';
  frame.counterAxisAlignItems = 'STRETCH';
  frame.primaryAxisSizingMode = 'AUTO';
  frame.counterAxisSizingMode = 'FIXED';
}

function setupHorizontalFrame(frame) {
  frame.layoutMode = 'HORIZONTAL';
  frame.itemSpacing = 0;
  frame.primaryAxisAlignItems = 'CENTER';
  frame.counterAxisAlignItems = 'CENTER';
  frame.primaryAxisSizingMode = 'FIXED';
  frame.counterAxisSizingMode = 'FIXED';
}

function createText(name, characters, fontSize, fill) {
  const text = figma.createText();
  text.name = name;
  text.characters = characters;
  text.fontSize = fontSize;
  text.fills = [fill];
  return text;
}

function createCenteredTextRow(name, characters, fontSize, fill, height) {
  const row = figma.createFrame();
  row.name = name;
  row.resize(263, height);
  row.fills = [];
  setupHorizontalFrame(row);
  row.appendChild(createText(`${name} Text`, characters, fontSize, fill));
  return row;
}

function createPillButton(name, label, fill, width, height, radius) {
  const button = figma.createFrame();
  button.name = name;
  button.resize(width, height);
  button.cornerRadius = radius;
  button.fills = [fill];
  button.layoutAlign = 'STRETCH';
  setupHorizontalFrame(button);
  button.appendChild(createText(`${name} Label`, label, 18, white));
  return button;
}

const root = figma.createFrame();
root.name = 'Daily Reward Popup';
root.x = 0;
root.y = 0;
root.resize(375, 812);
root.fills = [night];

const modal = figma.createFrame();
modal.name = 'Reward Modal';
modal.x = 32;
modal.y = 180;
modal.resize(311, 480);
modal.fills = [cardFill];
modal.cornerRadius = 32;
setupVerticalFrame(modal, 16, 24);
root.appendChild(modal);

const headerRow = figma.createFrame();
headerRow.name = 'Header Row';
headerRow.resize(263, 40);
headerRow.fills = [];
headerRow.layoutAlign = 'STRETCH';
headerRow.layoutMode = 'HORIZONTAL';
headerRow.primaryAxisAlignItems = 'MAX';
headerRow.counterAxisAlignItems = 'CENTER';
headerRow.primaryAxisSizingMode = 'FIXED';
headerRow.counterAxisSizingMode = 'FIXED';

const headerSpacer = figma.createFrame();
headerSpacer.name = 'Header Spacer';
headerSpacer.resize(1, 1);
headerSpacer.fills = [];
headerSpacer.layoutGrow = 1;
headerRow.appendChild(headerSpacer);
headerRow.appendChild(createPillButton('Close Button', 'X', closeBg, 40, 40, 20));
modal.appendChild(headerRow);

const badge = figma.createFrame();
badge.name = 'Day Badge';
badge.resize(119, 36);
badge.cornerRadius = 18;
badge.fills = [gold];
badge.layoutAlign = 'CENTER';
setupHorizontalFrame(badge);
badge.appendChild(createText('Day Badge Label', 'DAY 7', 16, ink));
modal.appendChild(badge);

modal.appendChild(createCenteredTextRow('Title', 'Daily Reward', 30, ink, 40));

const rewardIcon = figma.createFrame();
rewardIcon.name = 'Reward Icon';
rewardIcon.resize(119, 119);
rewardIcon.cornerRadius = 24;
rewardIcon.fills = [goldDark];
rewardIcon.layoutAlign = 'CENTER';
setupHorizontalFrame(rewardIcon);
rewardIcon.appendChild(createText('Coin Symbol', '$', 48, coin));
modal.appendChild(rewardIcon);

modal.appendChild(createCenteredTextRow('Reward Amount', '+500 Coins', 24, goldDark, 32));
modal.appendChild(createCenteredTextRow('Subtitle', 'Come back tomorrow for more', 14, muted, 24));
modal.appendChild(createPillButton('Claim Button', 'Claim Now', claim, 263, 56, 20));

return {
  rootId: root.id,
  modalId: modal.id,
  modalHeight: modal.height,
  childCount: modal.children.length
};
