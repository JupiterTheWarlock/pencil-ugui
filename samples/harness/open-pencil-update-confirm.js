const node = figma.getNodeById('0:15');
node.fills = [{ type: 'SOLID', color: { r: 0.125, g: 0.325, b: 0.937 } }];
return { id: node.id, name: node.name, fills: node.fills };
