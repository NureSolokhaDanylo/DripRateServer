const fs = require('fs');
const path = require('path');

const openapiPath = path.resolve(__dirname, '../openapi.json');
const outputDir = path.resolve(__dirname, '../sdks/typescript-custom');
const kotlinOutputDir = path.resolve(__dirname, '../sdks/kotlin-fabrikt');
const modelsFile = path.join(outputDir, 'models.ts');
const modelsJsFile = path.join(outputDir, 'models.js');
const apiFile = path.join(outputDir, 'api.ts');
const apiJsFile = path.join(outputDir, 'api.js');

if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

const spec = JSON.parse(fs.readFileSync(openapiPath, 'utf8'));

// Helper to convert OpenAPI schema to TypeScript
function schemaToTs(schema, isRequired = true) {
    if (!schema) return 'any';

    if (schema.$ref) {
        const refName = schema.$ref.split('/').pop();
        return refName;
    }

    if (schema.allOf) {
        return schema.allOf.map(s => schemaToTs(s)).join(' & ');
    }

    if (Array.isArray(schema.type)) {
        const types = schema.type.map(t => schemaToTs({ ...schema, type: t }));
        return types.join(' | ');
    }

    switch (schema.type) {
        case 'string':
            if (schema.enum) {
                return schema.enum.map(e => `'${e}'`).join(' | ');
            }
            return 'string';
        case 'number':
        case 'integer':
            return 'number';
        case 'boolean':
            return 'boolean';
        case 'array':
            return `Array<${schemaToTs(schema.items)}>`;
        case 'object':
            if (schema.properties) {
                const props = Object.entries(schema.properties).map(([name, prop]) => {
                    const required = schema.required && schema.required.includes(name);
                    return `  ${name}${required ? '' : '?'}: ${schemaToTs(prop, required)};`;
                });
                return `{\n${props.join('\n')}\n}`;
            }
            if (schema.additionalProperties) {
              return `Record<string, ${schemaToTs(schema.additionalProperties)}>`;
            }
            return 'Record<string, any>';
        case 'null':
            return 'null';
        default:
            return 'any';
    }
}

// Generate models.ts
const models = [];
const errorCodes = {};
const errorSchemas = {};

if (spec.components && spec.components.schemas) {
    for (const [name, schema] of Object.entries(spec.components.schemas)) {
        if (schema.type === 'object' && schema.properties) {
            const requiredFields = schema.required || [];
            const props = Object.entries(schema.properties).map(([propName, propSchema]) => {
                const isRequired = requiredFields.includes(propName);
                return `  ${propName}${isRequired ? '' : '?'}: ${schemaToTs(propSchema, isRequired)};`;
            });
            models.push(`export interface ${name} {\n${props.join('\n')}\n}`);
        } else if (schema.enum) {
            models.push(`export type ${name} = ${schema.enum.map(e => `'${e}'`).join(' | ')};`);
            if (name.endsWith('ErrorCode')) {
                errorCodes[name] = schema.enum;
            }
            errorSchemas[name] = schema.enum;
        } else {
            models.push(`export type ${name} = ${schemaToTs(schema)};`);
        }
    }
}

// Add ErrorCodes constant
let errorCodesJs = '';
if (Object.keys(errorCodes).length > 0) {
    const errorCodesConst = Object.entries(errorCodes).map(([name, values]) => {
        const props = values.map(v => `    ${v.split('.').pop()}: '${v}'`).join(',\n');
        return `  ${name.replace('ErrorCode', '')}: {\n${props}\n  }`;
    }).join(',\n');
    const tsConst = `export const ErrorCodes = {\n${errorCodesConst}\n} as const;`;
    models.push(tsConst);
    errorCodesJs = `export const ErrorCodes = {\n${errorCodesConst}\n};\n`;
}

fs.writeFileSync(modelsFile, models.join('\n\n') + '\n');
if (errorCodesJs) {
    fs.writeFileSync(modelsJsFile, errorCodesJs);
}

// Common Headers/Helpers
const getHeader = (isTs) => `
${isTs ? "import * as Models from './models';" : ""}

${isTs ? `export interface RequestOptions extends RequestInit {
  baseUrl?: string;
}` : ""}

async function handleResponse(response) {
  const isJson = response.headers.get('content-type')?.includes('application/json');
  const data = isJson ? await response.json() : await response.text();

  if (response.status >= 500) {
    throw new Error(\`Server error: \${response.status} \${response.statusText}\`);
  }

  const error = !response.ok ? data : null;
  return {
    status: response.status,
    data: response.ok ? data : null,
    error: error,
    errorCode: (error && typeof error === 'object' && error.code) ? error.code : null
  };
}

function buildUrl(path, params) {
  let url = path;
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      url = url.replace(\`{\${key}}\`, encodeURIComponent(String(value)));
    }
  });
  return url;
}

function buildQuery(params) {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      if (Array.isArray(value)) {
        value.forEach(v => query.append(key, String(v)));
      } else {
        query.append(key, String(value));
      }
    }
  });
  const queryString = query.toString();
  return queryString ? \`?\${queryString}\` : '';
}
`;

function generateApi(isTs) {
    let content = getHeader(isTs);
    const controllers = {};
    const errorDocs = [];

    for (const [path, methods] of Object.entries(spec.paths)) {
        for (const [method, operation] of Object.entries(methods)) {
            const tag = operation.tags ? operation.tags[0] : 'Default';
            if (!controllers[tag]) controllers[tag] = [];

            const operationId = operation.operationId || `${method}${path.replace(/\//g, '_')}`;
            let name = operationId.split('_').pop() || operationId;
            name = name.replace(/[^a-zA-Z0-9]/g, '');
            if (/^[0-9]/.test(name)) name = 'fn' + name;
            
            const parameters = operation.parameters || [];
            const pathParams = parameters.filter(p => p.in === 'path');
            const queryParams = parameters.filter(p => p.in === 'query');
            
            const bodyContent = operation.requestBody && operation.requestBody.content;
            const bodyType = bodyContent ? (bodyContent['application/json'] || bodyContent['multipart/form-data'] || Object.values(bodyContent)[0]) : null;
            const isMultipart = bodyContent && bodyContent['multipart/form-data'];

            const args = [];
            pathParams.forEach(p => args.push(isTs ? `${p.name}: ${schemaToTs(p.schema)}` : p.name));
            if (queryParams.length > 0) {
                args.push(isTs ? `query: { ${queryParams.map(p => `${p.name}${p.required ? '' : '?'}: ${schemaToTs(p.schema)}`).join(', ')} }` : 'query');
            }
            if (bodyType) {
                if (isTs) {
                    const schemaType = schemaToTs(bodyType.schema).replace(/Models\./g, '');
                    const prefixedType = schemaType.match(/^[A-Z]/) && !['Array', 'Record'].some(k => schemaType.startsWith(k)) ? `Models.${schemaType}` : schemaType;
                    args.push(`body: ${isMultipart ? 'FormData' : prefixedType}`);
                } else {
                    args.push('body');
                }
            }
            args.push(isTs ? 'options?: RequestOptions' : 'options');

            const methodErrorMappings = [];
            let returnType = '';
            if (isTs) {
                const responses = operation.responses || {};
                const responseTypes = Object.entries(responses)
                    .filter(([status]) => parseInt(status) < 500)
                    .map(([status, res]) => {
                        const content = res.content;
                        const schema = content ? (content['application/json'] || Object.values(content)[0]).schema : null;
                        const tsType = schema ? schemaToTs(schema).replace(/Models\./g, '') : 'any';
                        const prefixedType = tsType.match(/^[A-Z]/) && !['Array', 'Record'].some(k => tsType.startsWith(k)) ? `Models.${tsType}` : tsType;
                        
                        if (parseInt(status) < 300) {
                            return `{ status: ${status}, data: ${prefixedType}, error: null, errorCode: null }`;
                        } else {
                            const errorCodeType = tsType.includes('code?:') 
                                ? tsType.split('code?:')[1].split(';')[0].trim().replace(/Models\./g, '') 
                                : 'any';
                            const prefixedErrorCode = errorCodeType.match(/^[A-Z]/) ? `Models.${errorCodeType}` : errorCodeType;
                            
                            if (prefixedErrorCode !== 'any') {
                                methodErrorMappings.push({ status, code: prefixedErrorCode, values: errorSchemas[errorCodeType] || [] });
                            }

                            return `{ status: ${status}, data: null, error: ${prefixedType}, errorCode: ${prefixedErrorCode} }`;
                        }
                    });
                returnType = `: Promise<${responseTypes.length > 0 ? responseTypes.join(' | ') : 'any'}>`;
            }

            if (methodErrorMappings.length > 0) {
                errorDocs.push({ controller: tag, method: name, path, httpMethod: method.toUpperCase(), errors: methodErrorMappings });
            }

            const jsDoc = methodErrorMappings.length > 0 
                ? `  /**\n   * ${operation.summary || name}\n   * \n   * Errors:\n${methodErrorMappings.map(m => `   * - ${m.status}: ${m.code} (${m.values.join(', ')})`).join('\n')}\n   */\n`
                : '';

            const fetchOptions = [
                '...options',
                `method: '${method.toUpperCase()}'`,
                ...(isMultipart ? ['body'] : (bodyType ? ['body: JSON.stringify(body)'] : [])),
                `headers: { ${!isMultipart && bodyType ? "'Content-Type': 'application/json', " : ""}...options?.headers }`
            ];

            const func = `${jsDoc}  async ${name}(${args.join(', ')})${returnType} {
    const url = (options?.baseUrl || '') + buildUrl('${path}', { ${pathParams.map(p => p.name).join(', ')} }) + buildQuery(${queryParams.length > 0 ? 'query' : '{}'});
    const response = await fetch(url, {
      ${fetchOptions.join(',\n      ')}
    });
    return handleResponse(response);
  }`;
            controllers[tag].push(func);
        }
    }

    for (const [tag, funcs] of Object.entries(controllers)) {
        content += `\nexport const ${tag}Api = {\n${funcs.join(',\n')}\n};\n`;
    }

    if (isTs) {
        generateErrorMarkdown(errorDocs);
    }

    return content;
}

function generateErrorMarkdown(docs) {
    let md = '# API Error Documentation\n\n';
    md += 'This document lists all custom error codes mapped to their respective HTTP status codes for each API method.\n\n';

    const grouped = {};
    docs.forEach(d => {
        if (!grouped[d.controller]) grouped[d.controller] = [];
        grouped[d.controller].push(d);
    });

    for (const [controller, methods] of Object.entries(grouped)) {
        md += `## ${controller}Api\n\n`;
        md += '| Method | HTTP Method | Path | HTTP Status | Error Type | Error Codes |\n';
        md += '| :--- | :--- | :--- | :--- | :--- | :--- |\n';
        
        methods.forEach(m => {
            m.errors.forEach((e, idx) => {
                const methodName = idx === 0 ? `**${m.method}**` : '';
                const httpMethod = idx === 0 ? m.httpMethod : '';
                const path = idx === 0 ? `\`${m.path}\`` : '';
                md += `| ${methodName} | ${httpMethod} | ${path} | ${e.status} | \`${e.code}\` | ${e.values.map(v => `\`${v}\``).join(', ')} |\n`;
            });
        });
        md += '\n';
    }

    fs.writeFileSync(path.join(outputDir, 'ERRORS.md'), md);
    if (fs.existsSync(kotlinOutputDir)) {
        fs.writeFileSync(path.join(kotlinOutputDir, 'ERRORS.md'), md);
    }
}

fs.writeFileSync(apiFile, generateApi(true));
fs.writeFileSync(apiJsFile, generateApi(false));

console.log('SDK generated successfully in sdks/typescript-custom/');
