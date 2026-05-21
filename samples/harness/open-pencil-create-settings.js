const bg = { type: 'SOLID', color: { r: 0.063, g: 0.094, b: 0.153 } };
const white = { type: 'SOLID', color: { r: 1, g: 1, b: 1 } };
const ink = { type: 'SOLID', color: { r: 0.067, g: 0.094, b: 0.153 } };

const root = figma.createFrame();
root.name = 'Mobile Game Settings Panel';
root.x = 0;
root.y = 0;
root.resize(375, 812);
root.fills = [bg];

const card = figma.createFrame();
card.name = 'Settings Card';
card.x = 24;
card.y = 96;
card.resize(327, 520);
card.fills = [white];
card.cornerRadius = 28;
root.appendChild(card);

const title = figma.createText();
title.name = 'Title';
title.x = 32;
title.y = 32;
title.characters = 'Settings';
title.fontSize = 28;
title.fills = [ink];
card.appendChild(title);

const rows = [
  ['Music Toggle', 'Music Label', 'Music', 104, { r: 0.933, g: 0.949, b: 1 }],
  ['Sound Toggle', 'Sound Label', 'Sound', 176, { r: 0.941, g: 0.992, b: 0.957 }],
  ['Language Dropdown', 'Language Label', 'Language: English', 248, { r: 0.976, g: 0.980, b: 0.984 }]
];

for (const [boxName, labelName, label, y, color] of rows) {
  const box = figma.createRectangle();
  box.name = boxName;
  box.x = 32;
  box.y = y;
  box.resize(263, 56);
  box.cornerRadius = 16;
  box.fills = [{ type: 'SOLID', color }];
  card.appendChild(box);

  const text = figma.createText();
  text.name = labelName;
  text.x = 52;
  text.y = y + 17;
  text.characters = label;
  text.fontSize = 18;
  text.fills = [{ type: 'SOLID', color: { r: 0.122, g: 0.161, b: 0.216 } }];
  card.appendChild(text);
}

const close = figma.createRectangle();
close.name = 'Close Button';
close.x = 32;
close.y = 376;
close.resize(118, 56);
close.cornerRadius = 18;
close.fills = [{ type: 'SOLID', color: { r: 0.898, g: 0.906, b: 0.922 } }];
card.appendChild(close);

const confirm = figma.createRectangle();
confirm.name = 'Confirm Button';
confirm.x = 177;
confirm.y = 376;
confirm.resize(118, 56);
confirm.cornerRadius = 18;
confirm.fills = [{ type: 'SOLID', color: { r: 0.310, g: 0.275, b: 0.898 } }];
card.appendChild(confirm);

return { rootId: root.id, cardId: card.id, childCount: card.children.length };
