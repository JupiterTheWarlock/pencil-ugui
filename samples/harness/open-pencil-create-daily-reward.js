const night = { type: 'SOLID', color: { r: 0.08, g: 0.06, b: 0.18 } };
const cardFill = { type: 'SOLID', color: { r: 0.98, g: 0.97, b: 0.94 } };
const gold = { type: 'SOLID', color: { r: 0.95, g: 0.72, b: 0.18 } };
const goldDark = { type: 'SOLID', color: { r: 0.82, g: 0.55, b: 0.08 } };
const ink = { type: 'SOLID', color: { r: 0.15, g: 0.12, b: 0.22 } };
const muted = { type: 'SOLID', color: { r: 0.45, g: 0.42, b: 0.52 } };
const claim = { type: 'SOLID', color: { r: 0.92, g: 0.38, b: 0.12 } };
const closeBg = { type: 'SOLID', color: { r: 0.88, g: 0.86, b: 0.9 } };

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
modal.resize(311, 452);
modal.fills = [cardFill];
modal.cornerRadius = 32;
root.appendChild(modal);

const badge = figma.createRectangle();
badge.name = 'Day Badge';
badge.x = 96;
badge.y = 24;
badge.resize(119, 36);
badge.cornerRadius = 18;
badge.fills = [gold];
modal.appendChild(badge);

const badgeText = figma.createText();
badgeText.name = 'Day Badge Label';
badgeText.x = 118;
badgeText.y = 32;
badgeText.characters = 'DAY 7';
badgeText.fontSize = 16;
badgeText.fills = [ink];
modal.appendChild(badgeText);

const title = figma.createText();
title.name = 'Title';
title.x = 72;
title.y = 80;
title.characters = 'Daily Reward';
title.fontSize = 30;
title.fills = [ink];
modal.appendChild(title);

const rewardIcon = figma.createRectangle();
rewardIcon.name = 'Reward Icon';
rewardIcon.x = 96;
rewardIcon.y = 136;
rewardIcon.resize(119, 119);
rewardIcon.cornerRadius = 24;
rewardIcon.fills = [goldDark];
modal.appendChild(rewardIcon);

const coinIcon = figma.createText();
coinIcon.name = 'Coin Symbol';
coinIcon.x = 138;
coinIcon.y = 172;
coinIcon.characters = '$';
coinIcon.fontSize = 48;
coinIcon.fills = [{ type: 'SOLID', color: { r: 1, g: 0.92, b: 0.55 } }];
modal.appendChild(coinIcon);

const amount = figma.createText();
amount.name = 'Reward Amount';
amount.x = 108;
amount.y = 272;
amount.characters = '+500 Coins';
amount.fontSize = 24;
amount.fills = [goldDark];
modal.appendChild(amount);

const subtitle = figma.createText();
subtitle.name = 'Subtitle';
subtitle.x = 56;
subtitle.y = 312;
subtitle.characters = 'Come back tomorrow for more';
subtitle.fontSize = 14;
subtitle.fills = [muted];
modal.appendChild(subtitle);

const claimButton = figma.createRectangle();
claimButton.name = 'Claim Button';
claimButton.x = 24;
claimButton.y = 360;
claimButton.resize(263, 56);
claimButton.cornerRadius = 20;
claimButton.fills = [claim];
modal.appendChild(claimButton);

const claimLabel = figma.createText();
claimLabel.name = 'Claim Label';
claimLabel.x = 108;
claimLabel.y = 377;
claimLabel.characters = 'Claim Now';
claimLabel.fontSize = 18;
claimLabel.fills = [{ type: 'SOLID', color: { r: 1, g: 1, b: 1 } }];
modal.appendChild(claimLabel);

const closeButton = figma.createRectangle();
closeButton.name = 'Close Button';
closeButton.x = 255;
closeButton.y = 16;
closeButton.resize(40, 40);
closeButton.cornerRadius = 20;
closeButton.fills = [closeBg];
modal.appendChild(closeButton);

const closeLabel = figma.createText();
closeLabel.name = 'Close Label';
closeLabel.x = 267;
closeLabel.y = 24;
closeLabel.characters = 'X';
closeLabel.fontSize = 18;
closeLabel.fills = [muted];
modal.appendChild(closeLabel);

return {
  rootId: root.id,
  modalId: modal.id,
  childCount: modal.children.length
};
