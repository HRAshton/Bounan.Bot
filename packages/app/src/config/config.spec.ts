import { beforeEach, describe, expect, it, vi } from 'vitest';

const fetchSsmValueMock = vi.hoisted(() => vi.fn());

vi.mock('../../../../third-party/common/ts/runtime/ssm-client', () => ({ fetchSsmValue: fetchSsmValueMock }));

const loadModule = async () => {
  vi.resetModules();
  return await import('./config');
};

describe('config', () => {
  beforeEach(() => {
    fetchSsmValueMock.mockReset();
  });

  it('throws "Config not initialized" before initConfig is called', async () => {
    const { config } = await loadModule();
    expect(() => config.value).toThrow(/Config not initialized/i);
  });

  it('initConfig loads config from SSM and makes config.value available', async () => {
    const fakeConfig = {
      animan: { registerVideosLambdaName: 'lambda-name' },
      processing: { outdatedPeriodHours: 24 },
    };

    fetchSsmValueMock.mockResolvedValueOnce(fakeConfig);

    const { initConfig, config } = await loadModule();

    await initConfig();

    expect(fetchSsmValueMock).toHaveBeenCalledTimes(1);
    expect(fetchSsmValueMock).toHaveBeenCalledWith('/bounan/tofill/runtime-config');

    expect(config.value).toBe(fakeConfig); // same reference
  });

  it('initConfig can be called again and overwrites the cached config', async () => {
    const c1 = { processing: { outdatedPeriodHours: 24 } };
    const c2 = { processing: { outdatedPeriodHours: 48 } };

    fetchSsmValueMock.mockResolvedValueOnce(c1).mockResolvedValueOnce(c2);

    const { initConfig, config } = await loadModule();

    await initConfig();
    expect(config.value).toBe(c1);

    await initConfig();
    expect(config.value).toBe(c2);

    expect(fetchSsmValueMock).toHaveBeenCalledTimes(2);
  });

  it('if initConfig fails, it propagates the error and config.value stays uninitialized', async () => {
    fetchSsmValueMock.mockRejectedValueOnce(new Error('ssm down'));

    const { initConfig, config } = await loadModule();

    await expect(initConfig()).rejects.toThrow('ssm down');
    expect(() => config.value).toThrow(/Config not initialized/i);
  });
});
