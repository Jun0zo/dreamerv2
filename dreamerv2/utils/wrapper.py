import gym
import numpy as np
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple


class UnityEnv(gym.Env):
    metadata = {'render.modes': ['human', 'rgb_array']}

    def __init__(self, file_name=None, seed=None, no_graphics=False):
        # Unity 환경 초기화
        print("[*] Initializing Unity environment")
        self.env = UnityEnvironment(
            file_name=file_name, seed=seed, no_graphics=no_graphics)
        print("[*] Unity environment initialized")
        self.env.reset()
        print("[*] Unity environment reset")
        self.behavior_name = list(self.env.behavior_specs.keys())[0]
        self.spec = self.env.behavior_specs[self.behavior_name]

        # Action and Observation spaces based on Unity specs
        self.action_space = gym.spaces.Box(
            low=-1.0, high=1.0, shape=(self.spec.action_spec.continuous_size,), dtype=np.float32
        )

        print("Des ", self.spec.observation_specs[0].shape)
        # df
        self.observation_space = gym.spaces.Box(
            low=0, high=255, shape=self.spec.observation_specs[0].shape)
        print(f"observation_space: {self.observation_space}")

    def reset(self):
        # Unity 환경 재설정
        self.env.reset()
        decision_steps, _ = self.env.get_steps(self.behavior_name)
        obs = decision_steps.obs  # 관찰값
        out = self._process_observation(obs)[0]
        return out# only return camera

    def step(self, action):
        # 행동을 Unity ActionTuple 형식으로 변환
        action = ActionTuple(continuous=np.array([action], dtype=np.float32))

        # 에이전트 행동 설정 및 환경 업데이트
        agent_id = list(self.env.get_steps(self.behavior_name)[0].agent_id)[0]
        self.env.set_action_for_agent(self.behavior_name, agent_id, action)
        self.env.step()

        # 다음 스텝에서 관찰값, 보상, 완료 여부 얻기
        decision_steps, terminal_steps = self.env.get_steps(self.behavior_name)

        if len(terminal_steps) > 0:
            done = True
            obs = terminal_steps.obs
            reward = terminal_steps.reward[0]
        else:
            done = False
            obs = decision_steps.obs
            reward = decision_steps.reward[0]

        return self._process_observation(obs), reward, done, {}

    def _process_observation(self, obs):
        # 만약 이미지가 (H, W, C)라면, Gym과 호환되도록 (C, H, W)로 변환
        # But Unity에서 주는 값이 (C, H, W)로 주어지는 것 같아서 수정
        # if len(obs.shape) == 3:
        #     return obs.transpose(2, 0, 1)
        
        # return camera(84x84), (position x, y, z), (angle x, y, z) rotation x (프레임에 얼마나 떨렸는지), rotation z 
        camera_obs, other_obs = obs[0][0], obs[1][0]
        return camera_obs, \
                (other_obs[0], other_obs[1], other_obs[2]), \
                (other_obs[3], other_obs[4], other_obs[5]), \
                other_obs[6], other_obs[7]
        

    def close(self):
        # Unity 환경 닫기
        self.env.close()

    def seed(self, seed=None):
        # Unity 환경의 시드를 설정하려면 재초기화해야 합니다.
        self.env.close()
        self.__init__(seed=seed)


class ActionRepeat(gym.Wrapper):
    def __init__(self, env, repeat=1):
        super(ActionRepeat, self).__init__(env)
        self.repeat = repeat

    def step(self, action):
        done = False
        total_reward = 0
        current_step = 0
        while current_step < self.repeat and not done:
            obs, reward, done, info = self.env.step(action)
            total_reward += reward
            current_step += 1
        return obs, total_reward, done, info


class TimeLimit(gym.Wrapper):
    def __init__(self, env, duration):
        super(TimeLimit, self).__init__(env)
        self._duration = duration
        self._step = 0

    def step(self, action):
        assert self._step is not None, 'Must reset environment.'
        obs, reward, done, info = self.env.step(action)
        self._step += 1
        if self._step >= self._duration:
            done = True
            info['time_limit_reached'] = True
        return obs, reward, done, info

    def reset(self):
        self._step = 0
        return self.env.reset()


class OneHotAction(gym.Wrapper):
    def __init__(self, env):
        assert isinstance(
            env.action_space, gym.spaces.Discrete), "This wrapper only works with discrete action space"
        shape = (env.action_space.n,)
        env.action_space = gym.spaces.Box(
            low=0, high=1, shape=shape, dtype=np.float32)
        env.action_space.sample = self._sample_action
        super(OneHotAction, self).__init__(env)

    def step(self, action):
        index = np.argmax(action).astype(int)
        reference = np.zeros_like(action)
        reference[index] = 1
        return self.env.step(index)

    def reset(self):
        return self.env.reset()

    def _sample_action(self):
        actions = self.env.action_space.shape[0]
        index = np.random.randint(0, actions)
        reference = np.zeros(actions, dtype=np.float32)
        reference[index] = 1.0
        return reference
